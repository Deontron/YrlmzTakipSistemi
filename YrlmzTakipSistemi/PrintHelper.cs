using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace YrlmzTakipSistemi
{
    class PrintHelper
    {
        public void PrintDataGrid(DataGrid dataGrid)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();
                Table table = new Table();
                doc.Blocks.Add(table);

                foreach (var column in dataGrid.Columns)
                {
                    if (column.Visibility == Visibility.Visible)
                    {
                        table.Columns.Add(new TableColumn());
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
                            BorderThickness = new Thickness(1)
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
                                BorderThickness = new Thickness(1)
                            });
                        }
                    }
                }

                IDocumentPaginatorSource paginator = doc;
                printDialog.PrintDocument(paginator.DocumentPaginator, "DataGrid Yazdırma");
            }
        }
    }
}
