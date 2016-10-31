(function (diy) {

    // Enums
    var mappingTypes = {
        Singleton: 0,
        Instance: 1,
        Method: 2
    };

    // Private members
    var containers = {};

    // Static methods
    diy.getAllContainers = function () {
        return containers;
    };

    // Define container
    diy.Container = diy.Container || {};
    
    // Container constructor
    diy.Container.New = function (name) {

        // Argument exceptions
        if (name) {
            if (containers["name"]) {
                throw {
                    name: "DIYError",
                    message: "A container with the name '" + name + "' has already been created"
                };
            }
        }
        
        // Private fields
        var mappings = {};

        // Private methods
        var bind = function(key, constructor) {
            mappings[key] = { type: mappingTypes.Instance, data: constructor };
        };

        var bindSingleton = function(key, instance) {
            mappings[key] = { type: mappingTypes.Singleton, data: instance };
        };

        var bindMethod = function(key, method) {
            mappings[key] = { type: mappingTypes.Method, data: method };
        };

        // Public members
        var container = {};
        
        // Methods
        container.bind = bind;
        container.bindSingleton = bindSingleton;
        container.bindMethod = bindMethod;

        container.reset = function() {
            mappings = {};
        };

        container.getDependency = function(key) {
            var mapping = mappings[key];
            
            if (!mapping) {
                throw {
                    name: "DIYError",
                    message: "A binding was not found for argument '" + key + "' in the specified container"                  
                };
            }

            switch (mapping.type) {
                case mappingTypes.Singleton:
                    return mapping.data;
                case mappingTypes.Instance:
                    return mapping.data.DIY();
                case mappingTypes.Method:
                    return mapping.data();
            }
            
            throw {
                name: "DIYError",
                message: "One or more bindings more incorrectly configured"         
            };
        };
    
        // Add container to global collection
        if (name) {
            containers[name] = container;
        } else {
            containers.default = container;
        }
        
        return container;
    };
    
    // Extend function prototype for D.I in constructor functions
    Function.prototype.DIY = function (containerName) {

        var container;
        if (containerName) {
            container = containers[containerName];
            if (!container) {
                throw {
                    name: "DIYError",
                    message: "A container with the name '" + containerName + "' was not found, have you created one?"
                };
            }
        }
        else {
            container = containers.default;
            if (!container) {
                throw {
                    name: "DIYError",
                    message: "A default container was not found, have you created one?"
                };
            }
        }

        // Make sure we can handle Jasmine spies
        var obj = this;
        if (obj.isSpy) {
            obj = obj.originalValue;
        }

        // Decode the function header and extract parameters
        var string = obj.toString();
        var start = string.indexOf("(") + 1;
        var end = string.indexOf(")");

        var args = string.substring(start, end).split(",");
        var len = args.length;

        var params = [];

        // Determine all required dependencies
        if (args.length > 1 || args[0].length > 0) {
            for (var i = 0; i < len; i++) {
                params.push(container.getDependency(args[i].trim()));
            }
        }

        // Invoke the constructor with correct dependencies
        return this.apply({}, params);
    };

})(window.DIY = window.DIY || {});