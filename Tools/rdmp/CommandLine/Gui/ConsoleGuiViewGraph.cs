﻿using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataViewing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiViewGraph : ConsoleGuiSqlEditor
    {
        private readonly AggregateConfiguration aggregate;
        private GraphView graphView;
        private Tab graphTab;

        public ConsoleGuiViewGraph(IBasicActivateItems activator, AggregateConfiguration aggregate) :
            base
            (activator, new ViewAggregateExtractUICollection(aggregate) { TopX = null })
        {
            graphView = new GraphView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            TabView.AddTab(graphTab = new Tab("Graph",graphView), false);
            this.aggregate = aggregate;
        }


        protected override void OnQueryCompleted(DataTable dt)
        {
            base.OnQueryCompleted(dt);

            TabView.SelectedTab = graphTab;

            string valueColumnName;
                        
            try
            {
                valueColumnName = aggregate.GetQuerySyntaxHelper().GetRuntimeName(aggregate.CountSQL);
            }
            catch (Exception)
            {
                valueColumnName = "Count";
            }

            PopulateGraphResults(dt, valueColumnName, aggregate.GetAxisIfAny());
        }

        private void PopulateGraphResults(DataTable dt, string countColumnName, AggregateContinuousDateAxis axis)
        {
            //       if (chart1.Legends.Count == 0)
            //         chart1.Legends.Add(new Legend());

            // clear any lingering settings
            graphView.Reset();

            // Work out how much screen real estate we have
            var boundsWidth = graphView.Bounds.Width;
            var boundsHeight = graphView.Bounds.Height;

            if (boundsWidth == 0)
            {
                boundsWidth = TabView.Bounds.Width - 4;
            }
            if (boundsHeight == 0)
            {
                boundsHeight = TabView.Bounds.Height - 4;
            }

            // if no time axis then we have a regular bar chart
            if (axis == null)
            {
                if(dt.Columns.Count == 2)
                {
                    SetupBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
                }
                else
                {
                    SetupMultiBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
                }
            }
            else
            {
                SetupLineGraph(dt, axis,countColumnName, boundsWidth, boundsHeight);
            }

        }

        private void SetupLineGraph(DataTable dt, AggregateContinuousDateAxis axis, string countColumnName, int boundsWidth, int boundsHeight)
        {
            graphView.AxisY.Text = countColumnName;
            graphView.GraphColor = Driver.MakeAttribute(Color.White, Color.Black);

            var xIncrement = 1/(boundsWidth / (decimal)dt.Rows.Count);
            
            graphView.MarginBottom = 2;
            graphView.AxisX.Increment = xIncrement * 10;
            graphView.AxisX.ShowLabelsEvery = 1;
            graphView.AxisX.Text = axis.AxisIncrement.ToString();
            graphView.AxisX.LabelGetter = (v) =>
            {
                var x = (int)v.GraphSpace.X;
                return x < 0 || x >= dt.Rows.Count ? "" : dt.Rows[x][0].ToString();
            };

            decimal minY = 0M;
            decimal maxY = 1M;

            var colors = GetColors(dt.Columns.Count - 1);


            for(int i=1;i<dt.Columns.Count;i++)
            {

                var series = new PathAnnotation() { BeforeSeries = true, LineColor = colors[i - 1]};
                int row = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    var yVal = Convert.ToDecimal(dr[i]);

                    minY = Math.Min(minY, yVal);
                    maxY = Math.Max(maxY, yVal);

                    series.Points.Add(new PointD(row++, yVal));
                }

                graphView.Annotations.Add(series);
            }

            var yIncrement = boundsHeight/(maxY - minY);

            graphView.CellSize = new PointD(xIncrement, yIncrement);

            graphView.AxisY.LabelGetter = (v) => FormatValue(v.GraphSpace.Y,minY,maxY);
            graphView.MarginLeft = (uint)(Math.Max(FormatValue(maxY, minY, maxY).Length, FormatValue(minY, minY, maxY).Length)) + 1;


        }

        private void SetupBarSeries(DataTable dt,string countColumnName, int boundsWidth, int boundsHeight)
        {
            var barSeries = new BarSeries();

            var softStiple = new GraphCellToRender('\u2591');
            var mediumStiple = new GraphCellToRender('\u2592');

            int row = 0;
            int widestCategory = 0;

            decimal min = 0M;
            decimal max = 1M;

            foreach (DataRow dr in dt.Rows)
            {
                var label = dr[0].ToString();

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = "<Null>";
                }

                var val = Convert.ToDecimal(dr[1]);

                min = Math.Min(min, val);
                max = Math.Max(max, val);

                widestCategory = Math.Max(widestCategory, label.Length);

                barSeries.Bars.Add(new BarSeries.Bar(label, row++ % 2 == 0 ? softStiple : mediumStiple, val));
            }

            // show bars alphabetically (graph is rendered y=0 at bottom)
            barSeries.Bars = barSeries.Bars.OrderByDescending(b => b.Text).ToList();


            // Configure Axis, Margins etc

            // make sure whole graph fits on axis
            decimal xIncrement = (max - min) / (boundsWidth);

            // 1 bar per row of console
            graphView.CellSize = new PointD(xIncrement, 1);

            graphView.Series.Add(barSeries);
            graphView.AxisY.LabelGetter = barSeries.GetLabelText;
            barSeries.Orientation = Orientation.Horizontal;
            graphView.MarginBottom = 2;
            graphView.MarginLeft = (uint)widestCategory + 1;

            // work out how to space x axis without scrolling
            graphView.AxisX.Increment = 10 * xIncrement;
            graphView.AxisX.ShowLabelsEvery = 1;
            graphView.AxisX.LabelGetter = (v) => FormatValue(v.GraphSpace.X, min, max);
            graphView.AxisX.Text = countColumnName;

            graphView.AxisY.Increment = 1;
            graphView.AxisY.ShowLabelsEvery = 1;

            // scroll to the top of the bar chart so that the natural scroll direction (down) is preserved
            graphView.ScrollOffset = new PointD(0, barSeries.Bars.Count - boundsHeight + 4);
        }

        private List<Attribute> GetColors(int numberNeeded)
        {
            var colors = new Attribute[15];

            colors[0] = Driver.MakeAttribute(Color.Blue, Color.Black);
            colors[1] = Driver.MakeAttribute(Color.Green, Color.Black);
            colors[2] = Driver.MakeAttribute(Color.Cyan, Color.Black);
            colors[3] = Driver.MakeAttribute(Color.Red, Color.Black);
            colors[4] = Driver.MakeAttribute(Color.Magenta, Color.Black);
            colors[5] = Driver.MakeAttribute(Color.Brown, Color.Black);
            colors[6] = Driver.MakeAttribute(Color.Gray, Color.Black);
            colors[7] = Driver.MakeAttribute(Color.DarkGray, Color.Black);
            colors[8] = Driver.MakeAttribute(Color.BrightBlue, Color.Black);
            colors[9] = Driver.MakeAttribute(Color.BrightGreen, Color.Black);
            colors[10] = Driver.MakeAttribute(Color.BrighCyan, Color.Black);
            colors[11] = Driver.MakeAttribute(Color.BrightRed, Color.Black);
            colors[12] = Driver.MakeAttribute(Color.BrightMagenta, Color.Black);
            colors[13] = Driver.MakeAttribute(Color.BrightYellow, Color.Black);
            colors[14] = Driver.MakeAttribute(Color.White, Color.Black);

            var toReturn = new List<Attribute>();

            for (int i = 0; i < numberNeeded; i++)
            {
                toReturn.Add(colors[i % colors.Length]);
            }

            return toReturn;
        }

        private void SetupMultiBarSeries(DataTable dt, string countColumnName, int boundsWidth, int boundsHeight)
        {
            int numberOfBars = dt.Columns.Count - 1;
            var colors = GetColors(numberOfBars).ToArray();
            var mediumStiple = '\u2592';
            graphView.GraphColor = Driver.MakeAttribute(Color.White, Color.Black);

            var barSeries = new MultiBarSeries(numberOfBars, numberOfBars+1,1, colors);

            decimal min = 0M;
            decimal max = 1M;
            
            foreach (DataRow dr in dt.Rows)
            {
                var label = dr[0].ToString();

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = "<Null>";
                }
                var vals = dr.ItemArray.Skip(1).Select(Convert.ToDecimal).ToArray();

                barSeries.AddBars(label, mediumStiple, vals);

                foreach(var val in vals)
                {
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }

            // Configure Axis, Margins etc

            // make sure whole graph fits on axis
            decimal yIncrement = (max - min) / (boundsHeight);

            // 1 bar per row of console
            graphView.CellSize = new PointD(1, yIncrement);

            graphView.Series.Add(barSeries);
            graphView.MarginBottom = 2;
            graphView.MarginLeft = (uint)(Math.Max(FormatValue(max,min,max).Length, FormatValue(min, min, max).Length))+1;
            
            // work out how to space x axis without scrolling
            graphView.AxisY.Increment = yIncrement*5;
            graphView.AxisY.ShowLabelsEvery = 1;
            graphView.AxisY.LabelGetter = (v) => FormatValue(v.GraphSpace.Y, min, max);
            graphView.AxisY.Text = countColumnName;

            graphView.AxisX.Increment = numberOfBars+1;
            graphView.AxisX.ShowLabelsEvery = 1;
            graphView.AxisX.LabelGetter = (v) => barSeries.SubSeries.First().GetLabelText(v);
            graphView.AxisX.Text = dt.Columns[0].ColumnName;
        }
        private string FormatValue(decimal val, decimal min, decimal max)
        {
            if (val < min)
                return "";

            if (val > 1)
            {
                return val.ToString("N0");
            }

            if (val >= 0.01M)
                return val.ToString("N2");
            if (val > 0.0001M)
                return val.ToString("N4");
            if (val > 0.000001M)
                return val.ToString("N6");


            return val.ToString();
        }
    }
}
