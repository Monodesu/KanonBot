#!/bin/bash
#将UTF-8带BOM编码的文件转化为UTF-8无BOM格式
if [[ -z "$1" ]];then
    echo '用法：./rmbom.sh [folder | file]'
    echo '将UTF-8编码的文件转化为UTF-8无BOM格式'
    exit 1
fi


path=$1
find $path -type f -name "*" -print | xargs -i sed -i '1 s/^\xef\xbb\xbf//' {}
echo "Convert finish"
