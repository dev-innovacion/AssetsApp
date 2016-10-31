using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop;

using Excel = Microsoft.Office.Interop.Excel; 
using System.Drawing;
using System.IO;
using System.util;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text;
namespace RivkaAreas.Reports.Models
{
   public class CreateExcelDoc
    {
        private Excel.Application app = null;
        private Excel.Workbook workbook = null;
        private Excel.Worksheet worksheet = null;
        private Excel.Range workSheet_range = null;
        public CreateExcelDoc()
        {
            createDoc();
        }
        public void createDoc()
        {
            try
            {
                app = new Excel.Application();
                app.Visible = false;
                app.DisplayAlerts= false;
                app.ScreenUpdating = true;
               
                workbook = app.Workbooks.Add(1);
                worksheet = (Excel.Worksheet)workbook.Sheets[1];
            }
            catch (Exception e)
            {
                Console.Write("Error");
            }
            finally
            {
            }
        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;

            }
            finally
            {
                GC.Collect();
            }
        }
        public string saveDoc(String url,String path)
        {
            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    try
                    {
                        System.IO.Directory.Delete(path, true);
                    }
                    catch (Exception ex) {   }
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }
                if (!System.IO.Directory.Exists(path))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    catch (Exception ex) { }
                    // System.IO.Directory.CreateDirectory(absoluteurl);
                }
               
                object misValue = System.Reflection.Missing.Value;
             /*   try
                {
                   // workbook.SaveAs(url, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, true, Excel.XlSaveAsAccessMode.xlNoChange, misValue, misValue, misValue, misValue, misValue);
                    workbook.SaveAs(url, Excel.XlFileFormat.xlOpenXMLWorkbook, misValue, misValue, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution, true, misValue, misValue, misValue);
                    workbook.Close(true, misValue, misValue);
                    app.Quit();
                    releaseObject(worksheet);
                    releaseObject(workbook);
                    releaseObject(app);
                }
                catch (Exception ex) { return ex.ToString(); }*/
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(url);
                    workbook.Close(null, null, null);
                    app.Workbooks.Close();
                    app.Quit();

                }
                catch (Exception ex)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            System.IO.Directory.Delete(path, true);
                        }
                        catch { }
                        // System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    if (!System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                        catch{ }
                        // System.IO.Directory.CreateDirectory(absoluteurl);
                    }
                    workbook.Saved = true;
                    workbook.SaveCopyAs(url);
                    workbook.Close(null, null, null);
                    app.Workbooks.Close();
                    app.Quit();

                   
                }

                return url;
            }
            catch(Exception ex)
            {
                 return "saved error:" + ex.ToString();
            }
        }
        public void groupRows(int startRow, int endRow, string name, int level)
        {
            try
            {
                workSheet_range = worksheet.Rows[string.Format("{0}:{1}", startRow, endRow), Type.Missing] as Excel.Range;
                workSheet_range.Name = name;
                workSheet_range.Group(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                workSheet_range.OutlineLevel = level;
            }
            catch
            {

            }
        }
        public void createHeaders(int row, int col, string htext, string cell1,
        string cell2, int mergeColumns, string b, bool font, int size, string
        fcolor)
        {
            worksheet.Cells[row, col] = htext;
            try
            {
                workSheet_range = worksheet.get_Range(cell1, cell2);
                workSheet_range.Merge(mergeColumns);
               
            }
            catch { }
            switch (b)
            {
                case "YELLOW":
                    workSheet_range.Interior.Color = System.Drawing.Color.DodgerBlue.ToArgb();
                    break;
                case "FB":
                    workSheet_range.Interior.Color = System.Drawing.Color.CornflowerBlue;
                    break;
                case "GRAY":
                    workSheet_range.Interior.Color = System.Drawing.Color.Gray.ToArgb();
                    break;
                case "BLUE":
                    workSheet_range.Interior.Color = System.Drawing.Color.DarkBlue.ToArgb();
                    break;
                case "GAINSBORO":
                    workSheet_range.Interior.Color =System.Drawing.Color.Gainsboro.ToArgb();
                    break;
                case "Turquoise":
                    workSheet_range.Interior.Color =
            System.Drawing.Color.Turquoise.ToArgb();
                    break;
                case "PeachPuff":
                    workSheet_range.Interior.Color =System.Drawing.Color.PeachPuff.ToArgb();
                    break;
                default:
                    workSheet_range.Interior.Color = System.Drawing.Color.Azure;
                    break;
            }

            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Borders.Weight = 3d;
               workSheet_range.Font.Bold = font;
            workSheet_range.ColumnWidth = size;
            if (fcolor.Equals(""))
            {
                workSheet_range.Font.Color = System.Drawing.Color.Snow.ToArgb();
            }
            else
            {
                workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
            workSheet_range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
           
        }
      
        public void SetPicture()
        {
          /*  try
            {
                Excel.Range oRange = (Excel.Range)worksheet.Cells[1, 1];
                float Left = (float)((double)oRange.Left);
                float Top = (float)((double)oRange.Top);
                const float ImageSize = 100;
                worksheet.Shapes.AddPicture(HttpContext.Current.Server.MapPath("~") + "/Content/Images/cinepolislogo.jpg", Microsoft.Office.Interop.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, Left, Top, ImageSize + 100, ImageSize);
                oRange.RowHeight = ImageSize + 1;
            }
            catch { }*/
        }
        public void addFast(int minrow,int mincol,int maxrow,int maxcol,string[,] arr)
        {
            Excel._Worksheet sheet = app.ActiveSheet as Excel._Worksheet;
            Excel.Range sheetCells = sheet.Cells;
            Excel.Range cellFirst = sheetCells[minrow, mincol] as Excel.Range;
            Excel.Range cellLast = sheetCells[maxrow, maxcol] as Excel.Range;
            Excel.Range theRange = sheet.get_Range(cellFirst, cellLast);
            theRange.set_Value(Type.Missing, arr);
            theRange.Borders.Color = System.Drawing.Color.Black.ToArgb();
            theRange.Borders.Weight = 3d;
        }
        public void addData(int row, int col, string data,
            string cell1, string cell2, string format,string date=null)
        {
            worksheet.Cells[row, col] = data;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            
            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Borders.Weight = 3d;
            workSheet_range.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
            workSheet_range.EntireColumn.NumberFormat = "@";
            if(date!=null)
             workSheet_range.EntireColumn.NumberFormat = "[$-809]dd mmmm yyyy;@";

                      
        }
    }
}
