<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestPublishing.ascx.cs" Inherits="UmbracoExamine.Test.TESTING.TestPublishing" %>
<div class="testBox">

    <asp:Button runat="server" ToolTip="Publish a single node" ID="Button1" Text="Test Single Node Publish" onclick="TestSinglePublish_Click" />
    <asp:Button runat="server" ToolTip="Publish many individual nodes consecutively" ID="TestMultiplePublish" Text="Test Consecutive Publishing" onclick="TestMultiplePublish_Click" />
    <asp:Button runat="server" ToolTip="Publish many individual nodes concurrently using multiple threads" ID="TestConcurrentPublish" Text="Test Concurrent Publishing" onclick="TestConcurrentPublish_Click" />
    
</div>