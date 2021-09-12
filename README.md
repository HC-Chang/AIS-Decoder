# AIS-Decoder

Automatic Identification System (AIS) is a nautical instrument that is used to identificate other ships to prevent attacking from pirate. It is also used to record ship's track. Most of important, the International Maritime Organization's International Convention for the Safety of Life at Sea (SOLAS) requires AIS to be installed aboard international voyaging ships with 300 or more gross tonnage (GT), and all passenger ships regardless of size. However, the data is encrypted to avoid distorting. It is no picnic to get the informations from AIS raw data. 

In order to know what the information by recorded, I try to make a tool, AIS-Decoder, to decode the encrypted raw data. It can do more maritime research by analysing informations which are behide from AIS raw datas.

- Follow the "Data Type.txt",and you can use lib to decode some type of AIS message.

- Those you decode datas will save as "*.cvs". It is more convient to analysis with Excel.

- Test data can find in File "test data".

- Decode "Message Type": 1.2.3.5.18.19.24

- The field for each "Message Type" is declared at table 1.

- table 1

![Fields]

![main]

- AIS-Decoder

## Reference:
- [AIVDM/AIVDO protocol decoding]

[main]: pics/main.png
[Fields]: pics/Fields.png
[AIVDM/AIVDO protocol decoding]: https://gpsd.gitlab.io/gpsd/AIVDM.html