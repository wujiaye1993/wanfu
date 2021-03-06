"use strict";
/*创建了一个 SignalR 的 connection 对象*/
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/WeChatHub")
    .build();

/*connection 绑定了一个事件，该事件的名称和服务器 Send 方法中第一个参数的值相呼应*/
/*通过这种绑定，客户端就可以接收到服务器推送过来的消息*/
connection.on("Recv", function (body) {
    var li = document.createElement("li");
    li = $(li).text(
        "接收方账号：" + body.toUserName
        + "  发送方账号：" + body.fromUserName
        + "  发言人：" + body.userName
        + "  创建时间：" + body.createTime
        + "  消息内容：" + body.content)
    $("#msgList").append(li);
});

connection.start()
    .then(function () {
        console.log("SignalR 已连接");
    }).catch(function (err) {
        console.log(err);
    });

/*反之，通过 connection.invoke("send",xxx)，也可以将消息发送到服务器端的 Send 方法中，即客服发送消息给客户*/
$(document).ready(function () {
    $("#btnSend").on("click", function () {
        var fromUserName = $("#fromUserName").val();
        var toUserName = $("#toUserName").val();
        var content = $("#content").val();
        //var customMessageFromWeixin = $("#customMessageFromWeixin").val();
        console.log(fromUserName + ":" + content);
        connection.invoke("sendAsync", {
            "Type": 0,
            "FromUserName": fromUserName,
            "ToUserName": toUserName,

            "Content": content
        });
        console.log("已调用connection.invoke send ");
    });
});