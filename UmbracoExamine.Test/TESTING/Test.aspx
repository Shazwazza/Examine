<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="UmbracoExamine.Test.Testing.Test" %>

<%@ Register src="TestIndexing.ascx" tagname="TestIndexing" tagprefix="uc1" %>
<%@ Register src="TestProviders.ascx" tagname="TestProviders" tagprefix="uc2" %>
<%@ Register src="TestSearching.ascx" tagname="TestSearching" tagprefix="uc3" %>
<%@ Register src="TestPublishing.ascx" tagname="TestPublishing" tagprefix="uc4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
		html{margin:0;padding:0;}
		body{margin:0;padding:0;font-family:Trebuchet MS,Lucida Grande,verdana,arial;background-color:Black;}
		div{margin:0;padding:0;}
		.testContainer {padding:0;margin:0;text-align:center;}
		div.testBox {margin:10px; background-color:#EEEEEE; border:solid 1px #CCCCCC;padding:10px;}
		div.testTrace {border:solid 10px black;}
		table {font-size:12px;width:100%;border:solid 1px white;background-color:Black;border-spacing:0;}
		table td {margin:1px;padding:2px;border:solid 1px white;}
		table tr.head {background-color:Black;color:White;}
		table tr.head td {border-color:Black;font-weight:bold;}
		table tr.alt {background-color:#EEEEEE;}
		table tr {background-color:#FFFFFF;}
		table td.cat {width:20%;}
	</style>
</head>
<body>
    <form id="form1" runat="server">
		<div class="testContainer">
			<uc2:TestProviders ID="TestProviders1" runat="server" />
			<uc1:TestIndexing ID="TestIndexing1" runat="server" />		
			<uc3:TestSearching ID="TestSearching1" runat="server" />
			<uc4:TestPublishing ID="TestPublishing1" runat="server" />
		</div>
		<div class="testTrace">
			<asp:PlaceHolder runat="server" ID="TraceOutput"></asp:PlaceHolder>
		</div>
    </form>
</body>
</html>
