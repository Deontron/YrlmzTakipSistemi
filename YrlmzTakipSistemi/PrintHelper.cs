using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace YrlmzTakipSistemi
{
    class PrintHelper
    {
        public void PrintDataGrid(DataGrid dataGrid, string name)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();

                Paragraph title = new Paragraph(new Run(name))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(title);

                Table table = new Table();
                doc.Blocks.Add(table);

                table.CellSpacing = 0;
                table.BorderThickness = new Thickness(1);
                table.BorderBrush = Brushes.Black;

                int visibleColumnCount = dataGrid.Columns.Count(c => c.Visibility == Visibility.Visible);

                foreach (var column in dataGrid.Columns)
                {
                    if (column.Visibility == Visibility.Visible)
                    {
                        table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
                    }
                }

                TableRowGroup headerGroup = new TableRowGroup();
                table.RowGroups.Add(headerGroup);
                TableRow headerRow = new TableRow();
                headerGroup.Rows.Add(headerRow);

                foreach (var column in dataGrid.Columns)
                {
                    if (column.Visibility == Visibility.Visible)
                    {
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run(column.Header.ToString())))
                        {
                            FontWeight = FontWeights.Bold,
                            Padding = new Thickness(4),
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            TextAlignment = TextAlignment.Center
                        });
                    }
                }

                TableRowGroup bodyGroup = new TableRowGroup();
                table.RowGroups.Add(bodyGroup);

                foreach (var item in dataGrid.Items)
                {
                    DataGridRow rowControl = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                    if (rowControl == null || rowControl.Visibility != Visibility.Visible)
                        continue;

                    TableRow newRow = new TableRow();
                    bodyGroup.Rows.Add(newRow);

                    foreach (var column in dataGrid.Columns)
                    {
                        if (column.Visibility == Visibility.Visible)
                        {
                            TextBlock cellContent = column.GetCellContent(item) as TextBlock;
                            newRow.Cells.Add(new TableCell(new Paragraph(new Run(cellContent?.Text ?? "")))
                            {
                                Padding = new Thickness(4),
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(1),
                                TextAlignment = TextAlignment.Center
                            });
                        }
                    }
                }

                doc.PageWidth = printDialog.PrintableAreaWidth;
                doc.PagePadding = new Thickness(20);
                doc.ColumnWidth = printDialog.PrintableAreaWidth;

                IDocumentPaginatorSource paginator = doc;
                printDialog.PrintDocument(paginator.DocumentPaginator, "Yorulmaz Tablo");
            }
        }
    }
}
