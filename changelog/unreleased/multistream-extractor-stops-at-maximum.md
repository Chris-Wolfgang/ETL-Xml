type: fix

`XmlMultiStreamExtractor` stops before opening the next stream once `MaximumItemCount` is reached, instead of reading, disposing and discarding one stream past the limit.
