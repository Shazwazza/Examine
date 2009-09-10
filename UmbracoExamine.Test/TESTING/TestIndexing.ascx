<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestIndexing.ascx.cs" Inherits="UmbracoExamine.Test.TESTING.TestIndexing" %>
<div class="testBox">

	<asp:Button runat="server" ID="TestRebuildIndex" Text="Test Rebuilding Index" onclick="TestRebuildButton_Click" />
	<asp:Button runat="server" ID="TestIndexButton" Text="Test Re-Indexing Everything" onclick="TestIndexButton_Click" />
	
</div>