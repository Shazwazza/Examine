<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestIndexing.ascx.cs" Inherits="UmbracoExamine.Test.TESTING.TestIndexing" %>
<div class="testBox">

	<asp:Button runat="server" ID="TestRebuildIndex" ToolTip="Re-creates the entire index and indexes all content and media" Text="Test Rebuilding Index" onclick="TestRebuildButton_Click" />
	<asp:Button runat="server" ID="TestIndexContentButton" ToolTip="Re-indexes content, if the index doesn't exist, rebuilds the whole thing" Text="Test Re-Indexing Content" onclick="TestIndexContentButton_Click" />
	<asp:Button runat="server" ID="TestIndexMediaButton" ToolTip="Re-indexes media, if the index doesn't exist, rebuilds the whole thing" Text="Test Re-Indexing Media" onclick="TestIndexMediaButton_Click" />
	
</div>