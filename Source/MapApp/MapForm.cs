using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MapInfo.Mapping;
using MapInfo.Data;
using MapInfo.Engine;
using MapInfo.Tools;
using MapInfo.Mapping.Thematics;
using MapInfo.Styles;

namespace MapTestApp
{
    public partial class MapForm : Form
    {
        public MapForm()
        {
            InitializeComponent();
            mapControl1.Map.ViewChangedEvent += new MapInfo.Mapping.ViewChangedEventHandler(Map_ViewChangedEvent);
            Map_ViewChangedEvent(this, null);
        }

        void Map_ViewChangedEvent(object sender, MapInfo.Mapping.ViewChangedEventArgs e)
        {
            // Display the zoom level
            Double dblZoom = System.Convert.ToDouble(String.Format("{0:E2}", mapControl1.Map.Zoom.Value));
            if (statusStrip1.Items.Count > 0)
            {
                statusStrip1.Items[0].Text = "缩放: " + dblZoom.ToString() + " " + MapInfo.Geometry.CoordSys.DistanceUnitAbbreviation(mapControl1.Map.Zoom.Unit);
            }
        }

        private void MapForm1_Load(object sender, EventArgs e)
        {
            //加载地图
            string MapPath = Path.Combine(Application.StartupPath, @"map\map.mws");
            MapWorkSpaceLoader mwsLoader = new MapWorkSpaceLoader(MapPath);
            mapControl1.Map.Load(mwsLoader);
            mapControl1.Tools.LeftButtonTool = "Select";

            FeatureLayer fLayer = null;
            foreach (IMapLayer layer in mapControl1.Map.Layers)
            {
                fLayer = layer as FeatureLayer;
                LayerHelper.SetSelectable(layer, true);
            }
        }

        DataRow drSerachResult = null;

        private void btSearch_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbWords.Text.Trim()))
            {
                MessageBox.Show("请输入关键词！");
                tbWords.Focus();
            }
            else
            {
                foreach(IMapLayer layer in mapControl1.Map.Layers)
                {
                    if(layer is FeatureLayer)
                    {
                        Table table = (layer as FeatureLayer).Table;
                        foreach (Feature feature in (table as MapInfo.Data.ITableFeatureCollection))
                        {
                            if (feature.Geometry is MapInfo.Geometry.LegacyText)
                            {
                                if (tbWords.Text.Equals(((MapInfo.Geometry.LegacyText)(feature.Geometry)).Caption))
                                {
                                    //IResultSetFeatureCollection collect =new Resut

                                    SearchInfo si = MapInfo.Data.SearchInfoFactory.SearchWhere("1=0");
                                    IResultSetFeatureCollection fc = Session.Current.Catalog.Search(table, si); 
                                    fc.Add(feature);
                                    Session.Current.Selections.DefaultSelection.Clear(); //清除默认选中状态
                                    Session.Current.Selections.DefaultSelection.Add(fc); //选中
                                    mapControl1.Map.Center = feature.Geometry.Centroid; //定位到地图中心
                                    return; //找到直接返回！

                                }
                            }
                            
                        }
                    }
                    
                }
                

            }
        }

    }
}
