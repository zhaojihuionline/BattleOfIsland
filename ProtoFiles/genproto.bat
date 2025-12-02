@echo off
chcp 65001 >nul
echo 正在生成 C# Protobuf 文件...
protoc --csharp_out=../Assets/Scripts/Hotfix/Network/Protobuf/Generated *.proto
if %errorlevel% == 0 (
    echo Protobuf 文件生成成功！
) else (
    echo Protobuf 文件生成失败！
    pause
)