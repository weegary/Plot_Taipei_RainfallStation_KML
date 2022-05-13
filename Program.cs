// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Xml;
using System.Drawing;

Console.WriteLine("This program is written by Gary Wee.");
Console.WriteLine("Date: 2022 May 14");
Console.WriteLine("This program parse the rainfall station kml file downloaded from https://data.taipei and retrieve data into DataTable format, which could be further usage in the future.");
Console.WriteLine("Then plot the data and save into image format. (JPG)");

Console.WriteLine("");
Console.WriteLine("Programming Running...");
string fileName = "Taipei_RainfallStation.kml";
DataTable dt = GetRainfallStationInfo_DataTable(fileName);
PlotData(dt);
Console.WriteLine("End");


DataTable GetRainfallStationInfo_DataTable(string fileName)
{
    DataTable dt = new DataTable();
    dt.Columns.Add("Name");
    dt.Columns.Add("Station ID");

    DataColumn col_longitude = new DataColumn();
    col_longitude.ColumnName = "Longitude";
    col_longitude.DataType = typeof(double);
    dt.Columns.Add(col_longitude);

    DataColumn col_latitude = new DataColumn();
    col_latitude.ColumnName = "Latitude";
    col_latitude.DataType = typeof(double);
    dt.Columns.Add(col_latitude);

    XmlDocument doc = new XmlDocument();
    doc.Load(fileName);
    XmlNodeList placemarks = doc.GetElementsByTagName("Placemark");

    foreach (XmlElement station in placemarks)
    {
        DataRow row = dt.NewRow();
        string name = station.GetElementsByTagName("name")[0].InnerText;
        var simpleData = station.GetElementsByTagName("SimpleData");
        string stationID = "";
        double lon = 0, lat = 0;
        foreach (XmlNode node in simpleData)
        {
            if (node.Attributes["name"].Value == "STATIONID")
            {
                stationID = node.InnerText;
            }
            if (node.Attributes["name"].Value == "lat")
            {
                lat = Convert.ToDouble(node.InnerText);
            }
            if (node.Attributes["name"].Value == "lon")
            {
                lon = Convert.ToDouble(node.InnerText);
                break;
            }
        }
        row["Name"] = name;
        row["Station ID"] = stationID;
        row["Longitude"] = lon;
        row["Latitude"] = lat;
        dt.Rows.Add(row);
    }
    return dt;
}

void PlotData(DataTable dt)
{
    using Bitmap bmp = new(300, 600);
    using Graphics gfx = Graphics.FromImage(bmp);
    gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    using Pen pen = new(Color.Blue);
    double x_min = (double)dt.Select("Longitude = MIN(Longitude)").FirstOrDefault()["Longitude"];
    double x_max = (double)dt.Select("Longitude = MAX(Longitude)").FirstOrDefault()["Longitude"];
    double y_min = (double)dt.Select("Latitude = MIN(Latitude)").FirstOrDefault()["Latitude"];
    double y_max = (double)dt.Select("Latitude = MAX(Latitude)").FirstOrDefault()["Latitude"];
    double x_interval = x_max - x_min;
    double y_interval = y_max - y_min;

    for (int i = 0; i < dt.Rows.Count; i++)
    {
        double a = ((double)dt.Rows[i]["longitude"] - x_min) / (x_interval) * bmp.Width;
        double b = bmp.Height - (((double)dt.Rows[i]["latitude"] - y_min) / (y_interval) * bmp.Height);
        int x = (int)a;
        int y = (int)b;
        gfx.DrawEllipse(pen, x, y, 5, 5);

        // Select 4 points to verify if the plot is correct.
        if (dt.Rows[i]["Name"].ToString() == "桃源國中" || dt.Rows[i]["Name"].ToString() == "埤腹" || dt.Rows[i]["Name"].ToString() == "雙園" || dt.Rows[i]["Name"].ToString() == "東湖國小")
        {
            Font drawFont = new Font("Arial", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Green);
            gfx.DrawString(dt.Rows[i]["Name"].ToString(), drawFont, drawBrush, x-10, y);
            gfx.DrawString(Convert.ToDouble(dt.Rows[i]["Longitude"]).ToString("0.00"), drawFont, drawBrush, x - 10, y+10);
        }
    }
    bmp.Save("rainfall_station.jpg");

}