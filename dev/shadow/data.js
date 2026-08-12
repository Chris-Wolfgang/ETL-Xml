window.BENCHMARK_DATA = {
  "lastUpdate": 1786509618054,
  "repoUrl": "https://github.com/Chris-Wolfgang/ETL-Xml",
  "entries": {
    "Xml shadow workloads": [
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "67bbad3064c6f79c123b8a64da9cff6f9d31a344",
          "message": "Merge pull request #248 from Chris-Wolfgang/fix/189-gitleaks-arm64\n\nfix(scripts): select gitleaks arch for Linux arm64 in build-pr.ps1",
          "timestamp": "2026-08-12T02:54:27Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/67bbad3064c6f79c123b8a64da9cff6f9d31a344"
        },
        "date": 1786509616209,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1771856.310546875,
            "unit": "ns",
            "range": "± 7853.767063628813"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 855083.7141927084,
            "unit": "ns",
            "range": "± 4105.053984834902"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2782067.484375,
            "unit": "ns",
            "range": "± 113035.33223287351"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 29314966.75,
            "unit": "ns",
            "range": "± 696917.5322120707"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 185599866.11111107,
            "unit": "ns",
            "range": "± 912382.7550110673"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 83314087.33333333,
            "unit": "ns",
            "range": "± 1572240.9643144982"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 270550587,
            "unit": "ns",
            "range": "± 2305410.1474809204"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2933534658.6666665,
            "unit": "ns",
            "range": "± 6751449.045007623"
          }
        ]
      }
    ]
  }
}