<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestPublishing.ascx.cs" Inherits="UmbracoExamine.Test.TESTING.TestPublishing" %>
<div class="testBox">
    <p>
        This will test the indexing service when many nodes are being published consecutively<br />
        <asp:Button runat="server" ID="TestMultiplePublish" Text="Test Multi-Publishing" onclick="TestMultiplePublish_Click" />
    </p>
</div>