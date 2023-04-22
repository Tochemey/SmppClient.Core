SmppClient.Core
==============

[![build](https://github.com/Tochemey/SmppClient.Core/actions/workflows/ci.yml/badge.svg)](https://github.com/Tochemey/SmppClient.Core/actions/workflows/ci.yml)

SMPP Client for NetCore.

This class library implements the SMPP 3.4 protocol for use within .NetCore application. It can be used to build both
ESME and SMSC based software.

# Key Features

1. EsmeManager support mutiple binds of different types. Will round-robin on Transmitter and Transceiver bind.
2. Will log the Full PDU to stdout.
3. Reconnection support when connection drop. Implements the Enquire link rules
4. Supports Encoding for ASCII, Latin1 and UTF-16. Handles Default data coding rules
5. Event driven API so all the details are taken care of
6. Working console based test application.
