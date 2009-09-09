<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="UmbracoExamine.Test.Testing.Test" trace="true" %>

<%@ Register src="TestIndexing.ascx" tagname="TestIndexing" tagprefix="uc1" %>
<%@ Register src="TestProviders.ascx" tagname="TestProviders" tagprefix="uc2" %>
<%@ Register src="TestSearching.ascx" tagname="TestSearching" tagprefix="uc3" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
		html{margin:0;padding:0;}
		body{margin:0;padding:0;}
		div{margin:0;padding:0;}
		.testContainer {padding:0;margin:0;background-color:Black;text-align:center;}
		div.testBox {margin:10px; background-color:Silver; border:solid 1px #CC0000;padding:10px;}
	</style>
</head>
<body>
    <form id="form1" runat="server">
		<div class="testContainer">
			<br />
			<uc2:TestProviders ID="TestProviders1" runat="server" />
			<uc1:TestIndexing ID="TestIndexing1" runat="server" />		
			<uc3:TestSearching ID="TestSearching1" runat="server" />
			<br />
		</div>
    </form>
</body>
</html>
