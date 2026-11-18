type: fix

`XmlSingleStreamLoader<T>` no longer repairs the output after a failed or cancelled load. Previously the writer was disposed on the failure path, which closed the root element and left a complete, well-formed document holding only the records written so far. The buffered bytes are now flushed and the stream closed directly (when `LeaveOpen = false`), so the root element stays unclosed and the truncation is visible to any consumer that parses the file.
