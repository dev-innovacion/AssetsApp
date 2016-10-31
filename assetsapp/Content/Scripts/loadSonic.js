var loader = {
    width: 100,
    height: 100,

    stepsPerFrame: 1,
    trailLength: 1,
    pointDistance: .025,

    // strokeColor: '#05E2FF',
    strokeColor: '#ff8f32',

    fps: 30,

    setup: function () {
        this._.lineWidth = 2;
    },
    step: function (point, index) {

        var cx = this.padding + 50,
            cy = this.padding + 50,
            _ = this._,
            angle = (Math.PI / 180) * (point.progress * 360);

        this._.globalAlpha = Math.max(.5, this.alpha);

        _.beginPath();
        _.moveTo(point.x, point.y);
        _.lineTo(
            (Math.cos(angle) * 35) + cx,
            (Math.sin(angle) * 35) + cy
        );
        _.closePath();
        _.stroke();

        _.beginPath();
        _.moveTo(
            (Math.cos(-angle) * 32) + cx,
            (Math.sin(-angle) * 32) + cy
        );
        _.lineTo(
            (Math.cos(-angle) * 27) + cx,
            (Math.sin(-angle) * 27) + cy
        );
        _.closePath();
        _.stroke();

    },
    path: [
        ['arc', 50, 50, 40, 0, 360]
    ]
};

container = document.getElementById("loader");
d = document.createElement('div');
d.className = 'gLoading';

//a = new Sonic(loader);

//d.appendChild(a.canvas);
container.appendChild(d);

//a.canvas.style.marginTop = (150 - a.fullHeight) / 2 + 'px';
//a.canvas.style.marginLeft = (150 - a.fullWidth) / 2 + 'px';

//a.play();

function _loading(msg) {
    if (msg == "" || msg == undefined) msg = "Cargando...";
    jQuery(".modal_loading span").html(msg);
    jQuery(".modal_fade , .modal_loading").toggle();
}