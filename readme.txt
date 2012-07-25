Tabular is a .NET 3.5 library which provides a quick and easy way to generate text or html tables from your app. For example:

TableRenderer.RenderToConsole(
    new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles()
    .Select(x => new { x.Name, x.Extension, x.CreationTime })
);

It can also export to CSV and generate to a Winforms datagrid.