window.BENCHMARK_DATA = {
  "lastUpdate": 1786596167520,
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
      },
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
          "id": "4bf605669cbfc2c7a5c59f1f5c4b35166d8ee712",
          "message": "Merge pull request #262 from Chris-Wolfgang/fix/inspectcode-real-findings\n\nfix: resolve InspectCode findings that were previously suppressed, not fixed",
          "timestamp": "2026-08-13T02:13:24Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/4bf605669cbfc2c7a5c59f1f5c4b35166d8ee712"
        },
        "date": 1786596165305,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1789575.4915364583,
            "unit": "ns",
            "range": "± 22172.4037878224"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 900049.765625,
            "unit": "ns",
            "range": "± 11590.385477723403"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2903758.3411458335,
            "unit": "ns",
            "range": "± 49261.470811738116"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 31092683.25,
            "unit": "ns",
            "range": "± 5165961.044801593"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 184335045.7777778,
            "unit": "ns",
            "range": "± 323329.95686318085"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 91873597.66666667,
            "unit": "ns",
            "range": "± 2068823.0170459445"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 279828401,
            "unit": "ns",
            "range": "± 2781656.8285746537"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2924075152.6666665,
            "unit": "ns",
            "range": "± 4247624.781432599"
          }
        ]
      }
    ]
  }
}