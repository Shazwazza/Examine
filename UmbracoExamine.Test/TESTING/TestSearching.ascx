<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestSearching.ascx.cs" Inherits="UmbracoExamine.Test.TESTING.TestSearching" %>
<div class="testBox">
	<asp:TextBox runat="server" ID="SearchTextBox"></asp:TextBox>
	<br />
	<asp:Button runat="server" ID="TestSearch" Text="Test Search" onclick="TestSearch_Click"/>
</div>