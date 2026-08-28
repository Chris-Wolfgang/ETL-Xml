window.BENCHMARK_DATA = {
  "lastUpdate": 1787899361118,
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
          "id": "fc2b880d8fb41428aa74f65e9ba94669cbbc697c",
          "message": "Merge pull request #268 from Chris-Wolfgang/vNext\n\nRelease 0.8.0",
          "timestamp": "2026-08-13T20:40:08Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/fc2b880d8fb41428aa74f65e9ba94669cbbc697c"
        },
        "date": 1786682410012,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1809265.2252604167,
            "unit": "ns",
            "range": "± 10081.135680357029"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 876289.2333984375,
            "unit": "ns",
            "range": "± 6415.330734171567"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2837379.5052083335,
            "unit": "ns",
            "range": "± 194039.66306563732"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 30646121.416666668,
            "unit": "ns",
            "range": "± 1358326.805719456"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 183235620.44444442,
            "unit": "ns",
            "range": "± 314694.05908037897"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 88571837.05555554,
            "unit": "ns",
            "range": "± 125161.25748102802"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 276893626,
            "unit": "ns",
            "range": "± 1104260.7530302794"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2924792737,
            "unit": "ns",
            "range": "± 2455249.2544462774"
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
          "id": "653a677784fea378db6a06dfc7a65594c3e95c74",
          "message": "Merge pull request #269 from Chris-Wolfgang/chore/baseline-0.8.0\n\nchore(release): advance PackageValidation baseline to 0.8.0",
          "timestamp": "2026-08-14T19:18:25Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/653a677784fea378db6a06dfc7a65594c3e95c74"
        },
        "date": 1786767283121,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1833782.1770833333,
            "unit": "ns",
            "range": "± 1437.780200971163"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 854208.9490559896,
            "unit": "ns",
            "range": "± 6972.433376630059"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2907127.8854166665,
            "unit": "ns",
            "range": "± 59582.69130501545"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 29674654.166666668,
            "unit": "ns",
            "range": "± 415112.6649556519"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 186729792.7777778,
            "unit": "ns",
            "range": "± 2343096.9507007794"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 83460061.33333333,
            "unit": "ns",
            "range": "± 586308.3276379275"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 277648600,
            "unit": "ns",
            "range": "± 2621798.8565252293"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3012310168,
            "unit": "ns",
            "range": "± 1656601.1598622645"
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
          "id": "49fa24dc40bfcae06fac6cf9b7f4f52ba4075798",
          "message": "Merge pull request #271 from Chris-Wolfgang/maint/publicapi-analyzer-condition\n\nGate PublicApiAnalyzers on Exists('PublicAPI.*.txt') - parity with repo-template@623211a",
          "timestamp": "2026-08-20T01:48:39Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/49fa24dc40bfcae06fac6cf9b7f4f52ba4075798"
        },
        "date": 1787199545398,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1836915.9153645833,
            "unit": "ns",
            "range": "± 14021.28200177768"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 875330.7298177084,
            "unit": "ns",
            "range": "± 7777.422798199207"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2858982.9505208335,
            "unit": "ns",
            "range": "± 103652.86523563977"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28559076.8125,
            "unit": "ns",
            "range": "± 92602.04066300132"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 187398852,
            "unit": "ns",
            "range": "± 652041.9452052145"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 90400789.66666667,
            "unit": "ns",
            "range": "± 1365196.9652798579"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 273971821.3333333,
            "unit": "ns",
            "range": "± 1646707.9079303448"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2949012157,
            "unit": "ns",
            "range": "± 1739891.9569536494"
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
          "id": "d42fcbaec4072c0aa4d06cf1b9608f7ec4764080",
          "message": "Merge pull request #276 from Chris-Wolfgang/fix/inspectcode-publicapi-gate\n\nfix: replace UnusedAutoPropertyAccessor.Global silence with per-site attributes",
          "timestamp": "2026-08-21T01:16:29Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/d42fcbaec4072c0aa4d06cf1b9608f7ec4764080"
        },
        "date": 1787285998071,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1471433.4583333333,
            "unit": "ns",
            "range": "± 19339.79071019466"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 673440.95703125,
            "unit": "ns",
            "range": "± 5940.098562072473"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2218378.5755208335,
            "unit": "ns",
            "range": "± 50993.57473152612"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 22917333.885416668,
            "unit": "ns",
            "range": "± 37702.10490483533"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 149096253.83333334,
            "unit": "ns",
            "range": "± 4886977.016408458"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 66388774.833333336,
            "unit": "ns",
            "range": "± 192017.08999470974"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 218071256.33333334,
            "unit": "ns",
            "range": "± 1124870.0114138005"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2371345112,
            "unit": "ns",
            "range": "± 2774790.3419611724"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787372235888,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1853641.26171875,
            "unit": "ns",
            "range": "± 46463.75910602"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 897489.4440104166,
            "unit": "ns",
            "range": "± 4593.677791870329"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2864059.3125,
            "unit": "ns",
            "range": "± 120503.34128986721"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28456292.71875,
            "unit": "ns",
            "range": "± 18650.7108655418"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 189053023.66666666,
            "unit": "ns",
            "range": "± 1141019.105059053"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 89366184.16666667,
            "unit": "ns",
            "range": "± 1288890.301004059"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 276184599.3333333,
            "unit": "ns",
            "range": "± 1582370.1452673243"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2912209189,
            "unit": "ns",
            "range": "± 6983712.139012676"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787458750668,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1804508.4622395833,
            "unit": "ns",
            "range": "± 2332.8827954256603"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 941146.9886067709,
            "unit": "ns",
            "range": "± 12657.762386746332"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2970789.5833333335,
            "unit": "ns",
            "range": "± 52095.0212092206"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28966766.541666668,
            "unit": "ns",
            "range": "± 158869.17912380002"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 185655531,
            "unit": "ns",
            "range": "± 2888816.826473799"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 92153518.66666667,
            "unit": "ns",
            "range": "± 1527386.7362472687"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 280545546.6666667,
            "unit": "ns",
            "range": "± 1451680.8473160805"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2980853504.3333335,
            "unit": "ns",
            "range": "± 4936337.067796053"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787545331949,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1864886.8186848958,
            "unit": "ns",
            "range": "± 8444.34753954791"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 861298.5777994791,
            "unit": "ns",
            "range": "± 8881.329197735682"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2802164.78125,
            "unit": "ns",
            "range": "± 113380.37075416883"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28922622.791666668,
            "unit": "ns",
            "range": "± 440273.1778143946"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 196691787.44444442,
            "unit": "ns",
            "range": "± 190723.38628097577"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 88450034.83333333,
            "unit": "ns",
            "range": "± 471491.55521979753"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 278032164,
            "unit": "ns",
            "range": "± 2208950.7925861543"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2996282489.3333335,
            "unit": "ns",
            "range": "± 1659688.2166920186"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787631566295,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1799250.375,
            "unit": "ns",
            "range": "± 2926.109806548517"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 862372.9280598959,
            "unit": "ns",
            "range": "± 14929.277531027494"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2801270.4427083335,
            "unit": "ns",
            "range": "± 122195.85702102832"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28716205.833333332,
            "unit": "ns",
            "range": "± 24158.45562885631"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 186966376,
            "unit": "ns",
            "range": "± 719541.3968040794"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 84272722.6111111,
            "unit": "ns",
            "range": "± 275665.46778182115"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 270987683.3333333,
            "unit": "ns",
            "range": "± 2519076.3831476676"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2957668509.3333335,
            "unit": "ns",
            "range": "± 10690735.287084343"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787717977378,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1756439.0201822917,
            "unit": "ns",
            "range": "± 6785.966185743351"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 858036.0201822916,
            "unit": "ns",
            "range": "± 8720.838994805135"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2935812.9036458335,
            "unit": "ns",
            "range": "± 95001.07453311703"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 27943575.833333332,
            "unit": "ns",
            "range": "± 81820.56584127406"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 180957451,
            "unit": "ns",
            "range": "± 333674.2264256838"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 87980747.66666667,
            "unit": "ns",
            "range": "± 745000.7710322073"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 280566944.3333333,
            "unit": "ns",
            "range": "± 2494361.6680027246"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2922892923.6666665,
            "unit": "ns",
            "range": "± 9453087.855652053"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787811311108,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1828450.9186197917,
            "unit": "ns",
            "range": "± 9722.90504882781"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 890442.1761067709,
            "unit": "ns",
            "range": "± 12266.799221219611"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2941386.7604166665,
            "unit": "ns",
            "range": "± 81402.47175494599"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 29014420.625,
            "unit": "ns",
            "range": "± 9824.504956665995"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 188717667.44444442,
            "unit": "ns",
            "range": "± 120362.84253241241"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 88113369.22222222,
            "unit": "ns",
            "range": "± 754414.5421489532"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 278087612,
            "unit": "ns",
            "range": "± 1794825.2305648036"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3002874669.3333335,
            "unit": "ns",
            "range": "± 796816.765047858"
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
          "id": "60fae5bae3b685a392dbbb551c7ce40a962a3e79",
          "message": "Merge pull request #278 from Chris-Wolfgang/chore/baseline-0.8.1\n\nchore(release): advance PackageValidation baseline to 0.8.1",
          "timestamp": "2026-08-21T18:17:40Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/60fae5bae3b685a392dbbb551c7ce40a962a3e79"
        },
        "date": 1787899357878,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1438165.1451822917,
            "unit": "ns",
            "range": "± 7189.724681284885"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 697564.5735677084,
            "unit": "ns",
            "range": "± 4337.0294081511665"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2643549.7447916665,
            "unit": "ns",
            "range": "± 305821.5464336504"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 22870099.208333332,
            "unit": "ns",
            "range": "± 25410.155583384567"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 146913162.58333334,
            "unit": "ns",
            "range": "± 1128850.4786393694"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 67452711.88888888,
            "unit": "ns",
            "range": "± 509015.0927227894"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 220165983.16666666,
            "unit": "ns",
            "range": "± 1530127.3960668547"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2362944068,
            "unit": "ns",
            "range": "± 3058158.97333739"
          }
        ]
      }
    ]
  }
}