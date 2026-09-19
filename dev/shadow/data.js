window.BENCHMARK_DATA = {
  "lastUpdate": 1789791666304,
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
          "id": "a55cd59e4f30de51fcbcd250d917b4633d7e6e24",
          "message": "Merge pull request #287 from Chris-Wolfgang/dependabot/github_actions/github-actions-2390256866\n\nchore(deps): bump the github-actions group across 1 directory with 4 updates",
          "timestamp": "2026-09-05T15:37:20Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/a55cd59e4f30de51fcbcd250d917b4633d7e6e24"
        },
        "date": 1788668477421,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1613407.744140625,
            "unit": "ns",
            "range": "± 16412.344038075662"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 833936.7083333334,
            "unit": "ns",
            "range": "± 34601.143976641695"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2765971.640625,
            "unit": "ns",
            "range": "± 110955.2438536406"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 25604610.979166668,
            "unit": "ns",
            "range": "± 282995.1488635573"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 161934984.41666666,
            "unit": "ns",
            "range": "± 891087.1348810863"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 82809575.61904761,
            "unit": "ns",
            "range": "± 2718079.4619316086"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 249967553.66666666,
            "unit": "ns",
            "range": "± 8489067.84910018"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2582145727.3333335,
            "unit": "ns",
            "range": "± 1891228.9375129424"
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
          "id": "a55cd59e4f30de51fcbcd250d917b4633d7e6e24",
          "message": "Merge pull request #287 from Chris-Wolfgang/dependabot/github_actions/github-actions-2390256866\n\nchore(deps): bump the github-actions group across 1 directory with 4 updates",
          "timestamp": "2026-09-05T15:37:20Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/a55cd59e4f30de51fcbcd250d917b4633d7e6e24"
        },
        "date": 1788755103610,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1903316.9720052083,
            "unit": "ns",
            "range": "± 5277.54064279854"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 1006397.0813802084,
            "unit": "ns",
            "range": "± 27489.493806627903"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 3028075.4453125,
            "unit": "ns",
            "range": "± 164326.54827666673"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 29388974.104166668,
            "unit": "ns",
            "range": "± 270004.19016084983"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 188940372.66666666,
            "unit": "ns",
            "range": "± 959156.101434368"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 92177958.83333333,
            "unit": "ns",
            "range": "± 597881.0176657921"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 288217062.3333333,
            "unit": "ns",
            "range": "± 12106520.52520105"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3005478586.3333335,
            "unit": "ns",
            "range": "± 18951222.64238274"
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
          "id": "a55cd59e4f30de51fcbcd250d917b4633d7e6e24",
          "message": "Merge pull request #287 from Chris-Wolfgang/dependabot/github_actions/github-actions-2390256866\n\nchore(deps): bump the github-actions group across 1 directory with 4 updates",
          "timestamp": "2026-09-05T15:37:20Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/a55cd59e4f30de51fcbcd250d917b4633d7e6e24"
        },
        "date": 1788841297253,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1903044.1243489583,
            "unit": "ns",
            "range": "± 47406.22943623482"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 894032.7962239584,
            "unit": "ns",
            "range": "± 28386.14040553399"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2760910.0065104165,
            "unit": "ns",
            "range": "± 34530.25798310757"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 30138266.833333332,
            "unit": "ns",
            "range": "± 644883.4836166789"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 189017836,
            "unit": "ns",
            "range": "± 184360.98473767022"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 83233164,
            "unit": "ns",
            "range": "± 815603.3099260019"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 276258787.6666667,
            "unit": "ns",
            "range": "± 1795931.082542516"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3040558905,
            "unit": "ns",
            "range": "± 21059878.666417595"
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
          "id": "a55cd59e4f30de51fcbcd250d917b4633d7e6e24",
          "message": "Merge pull request #287 from Chris-Wolfgang/dependabot/github_actions/github-actions-2390256866\n\nchore(deps): bump the github-actions group across 1 directory with 4 updates",
          "timestamp": "2026-09-05T15:37:20Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/a55cd59e4f30de51fcbcd250d917b4633d7e6e24"
        },
        "date": 1788927719146,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1401839.1106770833,
            "unit": "ns",
            "range": "± 6189.775798318552"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 665267.6155598959,
            "unit": "ns",
            "range": "± 10798.735100475918"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2547434.0364583335,
            "unit": "ns",
            "range": "± 225579.9336010052"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 22415731.53125,
            "unit": "ns",
            "range": "± 101262.04250677326"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 144262273.75,
            "unit": "ns",
            "range": "± 1288543.8374100092"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 66376166.041666664,
            "unit": "ns",
            "range": "± 255026.19023844678"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 213898412.33333334,
            "unit": "ns",
            "range": "± 1117090.0235712132"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2316351575.6666665,
            "unit": "ns",
            "range": "± 16663338.760913562"
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
          "id": "ded4a70d14f929469bbd571e078ee3afb1751296",
          "message": "Release v0.9.0 — options records for all four stages inherit the Abstractions 0.24 base records; 15 superseded constructors hidden; IsDryRun setters deprecated (#302)\n\n* feat: make logger optional on the multi-stream ctors, defaulting to NullLogger\n\nCompletes the constructor convergence begun on the single-stream types: every\nextractor and loader in this package now takes the logger last and optional,\nmatching the fleet-wide convention already followed by Etl-DbClient.\n\nSix required-logger constructors become optional:\n  XmlMultiStreamExtractor<T>(IEnumerable<Stream>, ILogger<T>? = null)\n  XmlMultiStreamExtractor<T>(IEnumerable<Stream>, XmlReaderSettings, ILogger<T>? = null)\n  XmlMultiStreamLoader<T>(Func<TRecord, Stream>, ILogger<T>? = null)\n  XmlMultiStreamLoader<T>(Func<TRecord, Stream>, XmlWriterSettings, ILogger<T>? = null)\n  XmlMultiStreamLoader<T>(Func<TRecord, IBufferWriter<byte>>, ILogger<T>? = null)\n  XmlMultiStreamLoader<T>(Func<TRecord, IBufferWriter<byte>>, XmlWriterSettings, ILogger<T>? = null)\n\nnull (or omitted) now resolves to NullLogger.Instance instead of throwing\nArgumentNullException. A useful side effect: reader/writer settings can now be\nsupplied WITHOUT also supplying a logger, which previously was not possible.\n\nNot a breaking change: each parameter list is unchanged, so the emitted\nsignatures are identical. Release build with TreatWarningsAsErrors is clean and\nPackageValidation passes, so the 6 PublicAPI.Shipped.txt entries were corrected\nin place rather than recorded as an add/remove pair.\n\nNo overload became ambiguous: the shorter overloads still win resolution\nbecause all of their parameters have a corresponding argument, while the longer\nforms now require default substitution.\n\nTests: the three tests asserting a null logger throws now assert the NullLogger\ncontract. 330 unit + 9 doc-example tests pass in Release with\nTreatWarningsAsErrors.\n\nCo-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>\n\n* chore: put the logger last on the internal test-injection ctors\n\nApplies Rule 6 of the fleet constructor standard: the logger is the final\nparameter on EVERY constructor, internal ones included.\n\n  multi-stream:  (…, settings, ILogger? logger, IProgressTimer timer)\n              -> (…, settings, IProgressTimer timer, ILogger? logger = null)\n\n  single-stream: (…, settings, ILogger? logger, IProgressTimer timer, Options? options = null)\n              -> (…, settings, Options? options, IProgressTimer timer, ILogger? logger = null)\n\nThe single-stream overloads needed the fuller reorder because `options` was\ntrailing; it moves ahead of the timer so the logger can be last, matching the\ncanonical Rule 6 shape (inputs, options, timer, logger).\n\nInternal-only: no public API change, no PublicAPI entry, no consumer impact and\nnothing to deprecate. Test call sites updated, including supplying the now\nnon-defaulted `options` argument on the single-stream internal constructors.\n\n330 unit + 9 doc-example tests pass in Release with TreatWarningsAsErrors.\n\nCo-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>\n\n* refactor: put the timer before options on the single-stream internal ctors\n\nRevises the order this PR originally used, following call-site evidence rather\nthan the existing majority.\n\n  (…, settings, Options? options, IProgressTimer timer, ILogger? logger = null)\n    -> (…, settings, IProgressTimer timer, Options? options = null, ILogger? logger = null)\n\nThe first version made `options` a required positional parameter so that the\nlogger could stay last. That forced `options: null` at 14 call sites to satisfy\na parameter none of them actually set - all 14 are now gone.\n\nOrdering is driven by how these constructors are called across the fleet\n(149 internal call sites):\n\n  IProgressTimer   149/149 pass one -> required, and the reason the overload exists\n  ILogger           34/149 pass a real logger -> optional\n  options            0/14  pass non-null -> optional\n\nTimer first is also what makes the trailing parameters optional at all: an\noptional parameter cannot precede a required one (CS1737).\n\nThe multi-stream internal constructors already matched this shape and are\nunchanged.\n\nTest call sites that passed a logger positionally now name it, since the fourth\npositional slot is `options`.\n\n330 unit + 9 doc-example tests pass in Release with TreatWarningsAsErrors.\n\nCo-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>\n\n* feat(options): options records for all four stages, inheriting the Abstractions 0.24 base records (ADR-0009, part 1) — draft until 0.24.0 publishes (#297)\n\n* feat(options): options records for all four stages inheriting the Abstractions 0.24 base records (ADR-0009, part 1)\n\nXmlSingleStreamExtractorOptions and XmlSingleStreamLoaderOptions become\nsealed records (they were classes) inheriting ExtractorOptions /\nLoaderOptions, and gain ReaderSettings / WriterSettings plus, on the\nloader, IsDryRun. New XmlMultiStreamExtractorOptions and\nXmlMultiStreamLoaderOptions carry the same for the multi-stream stages. The\nreader/writer settings travel on the record the way Json carries\nSerializerOptions - nested as the XmlReaderSettings / XmlWriterSettings\ninstance, not flattened.\n\nThe single-stream stages' private core constructors chain base(options)\nand take the settings from the record when none were passed positionally;\nthe multi-stream stages gain (source, options, logger = null) record\nconstructors (streams; streamFactory; bufferWriterFactory) that chain\nbase(options). options is optional: every existing call keeps binding\nwhere it binds today (a compile-time guard test covers the positional-null\nshapes). The settings-taking and single-argument constructors are unchanged\nhere; hiding them is part 2.\n\nISupportDryRun dropped from both loaders (Abstractions 0.24 removes it);\nthe two dry-run contract tests use the now non-generic TestKit base.\nAbstractions / ErrorPolicies / TestKit / TestKit.Xunit 0.23.2 -> 0.24.0 -\nnot published yet; built against the local feed, PR stays draft.\n\nPublicAPI: 47 added entries (the converted records' synthesized members\nincluded; derived-record <Clone>$ lines left out as unmatchable); ten\npre-existing unrecorded XmlReport members re-surfaced and tracked in #296.\nApiCompat: CP0008 x10 for ISupportDryRun; the class->record conversion\nreported no break. Tests: XmlOptionsRecordTests (9 cases). CHANGELOG\nAdded / Changed / Removed.\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n* chore(deps): the examples project references the 0.24.0 family too (missed in 861d29b)\n\nCo-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>\n\n* feat(options): hide the fifteen superseded ctors; deprecate the IsDryRun setters; README + migration guide (ADR-0009, part 2) (#298)\n\nFifteen constructors get [EditorBrowsable(Never)] and a remarks block and\nare retained permanently: the single-argument ones from #253 / #281 and\nevery overload that took XmlReaderSettings / XmlWriterSettings or a logger\npositionally, all superseded by (source, options, logger) with the\nsettings on the record. Not [Obsolete]: positional calls bind to them by\nexact match, so a warning could only be silenced by rewriting the call,\nand removal is a MissingMethodException for un-rebuilt callers. The\n(source, logger = null) and (source, options, logger = null) overloads\nstay visible.\n\nThe two IsDryRun setters are [Obsolete] on the accessor (reads stay\nclean); the constructor assignments sit under a CS0618 pragma as the\nsupported replacement; the four test initializers configure IsDryRun\nthrough the records.\n\nREADME: the \"Constructor overloads\" section describes the record shape\nwith an example and the four records' members. docs/migrations/\nv0.8-to-v0.9.md is the first real migration guide in this repo. CHANGELOG\nChanged / Deprecated. No PublicAPI text change.\n\nNot done: the internal timer-injection constructors still take the\nsettings positionally (14 test call sites, one of which asserts the null\nguard); converting them to the record is a follow-up if wanted.\n\nCo-authored-by: Claude Fable 5.1 <noreply@anthropic.com>\n\n* chore: merge main into vNext (Dependabot #287/#288/#289) ahead of the 0.9.0 release (#300)\n\n* chore(deps): bump the github-actions group across 1 directory with 4 updates\n\nBumps the github-actions group with 4 updates in the / directory: [github/codeql-action/init](https://github.com/github/codeql-action), [github/codeql-action/analyze](https://github.com/github/codeql-action), [github/codeql-action/upload-sarif](https://github.com/github/codeql-action) and [softprops/action-gh-release](https://github.com/softprops/action-gh-release).\n\n\nUpdates `github/codeql-action/init` from 4.37.7 to 4.37.9\n- [Release notes](https://github.com/github/codeql-action/releases)\n- [Changelog](https://github.com/github/codeql-action/blob/main/CHANGELOG.md)\n- [Commits](https://github.com/github/codeql-action/compare/ff2f1c621b7f889edc0d3c761ac2e6a3f8cdb0dd...cdf488f595d80d6e07e03d4674febd5ab45fa938)\n\nUpdates `github/codeql-action/analyze` from 4.37.7 to 4.37.9\n- [Release notes](https://github.com/github/codeql-action/releases)\n- [Changelog](https://github.com/github/codeql-action/blob/main/CHANGELOG.md)\n- [Commits](https://github.com/github/codeql-action/compare/ff2f1c621b7f889edc0d3c761ac2e6a3f8cdb0dd...cdf488f595d80d6e07e03d4674febd5ab45fa938)\n\nUpdates `github/codeql-action/upload-sarif` from 4.37.7 to 4.37.9\n- [Release notes](https://github.com/github/codeql-action/releases)\n- [Changelog](https://github.com/github/codeql-action/blob/main/CHANGELOG.md)\n- [Commits](https://github.com/github/codeql-action/compare/ff2f1c621b7f889edc0d3c761ac2e6a3f8cdb0dd...cdf488f595d80d6e07e03d4674febd5ab45fa938)\n\nUpdates `softprops/action-gh-release` from 3.0.2 to 3.0.3\n- [Release notes](https://github.com/softprops/action-gh-release/releases)\n- [Changelog](https://github.com/softprops/action-gh-release/blob/master/CHANGELOG.md)\n- [Commits](https://github.com/softprops/action-gh-release/compare/3d0d9888cb7fd7b750713d6e236d1fcb99157228...efb35369e0ad2afab669f228072c1b0d510eae64)\n\n---\nupdated-dependencies:\n- dependency-name: github/codeql-action/init\n  dependency-version: 4.37.9\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: github-actions\n- dependency-name: github/codeql-action/analyze\n  dependency-version: 4.37.9\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: github-actions\n- dependency-name: github/codeql-action/upload-sarif\n  dependency-version: 4.37.9\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: github-actions\n- dependency-name: softprops/action-gh-release\n  dependency-version: 3.0.3\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: github-actions\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\n\n* Bump the dotnet-dependencies group with 7 updates\n\nBumps Meziantou.Analyzer from 3.0.172 to 3.0.201\nBumps Roslynator.Analyzers from 4.16.1 to 5.0.0\nBumps SonarAnalyzer.CSharp from 10.32.0.713 to 10.33.0.1635\nBumps Wolfgang.Etl.Abstractions from 0.23.2 to 0.23.4\nBumps Wolfgang.Etl.ErrorPolicies from 0.23.2 to 0.23.4\nBumps Wolfgang.Etl.TestKit from 0.23.2 to 0.23.4\nBumps Wolfgang.Etl.TestKit.Xunit from 0.23.2 to 0.23.4\n\n---\nupdated-dependencies:\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.201\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Roslynator.Analyzers\n  dependency-version: 5.0.0\n  dependency-type: direct:production\n  update-type: version-update:semver-major\n  dependency-group: dotnet-dependencies\n- dependency-name: SonarAnalyzer.CSharp\n  dependency-version: 10.33.0.1635\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.Abstractions\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.ErrorPolicies\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.ErrorPolicies\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.TestKit\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.TestKit\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Wolfgang.Etl.TestKit.Xunit\n  dependency-version: 0.23.4\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\n\n* Bump the dotnet-dependencies group with 7 updates\n\nBumps Meziantou.Analyzer from 3.0.201 to 3.0.234\nBumps Microsoft.Bcl.AsyncInterfaces from 10.0.11 to 10.0.12\nBumps Microsoft.Extensions.Logging.Abstractions from 10.0.11 to 10.0.12\nBumps Microsoft.Extensions.Logging.Console from 10.0.11 to 10.0.12\nBumps Microsoft.SourceLink.GitHub from 10.0.400 to 10.0.401\nBumps SonarAnalyzer.CSharp from 10.33.0.1635 to 10.34.0.3385\nBumps System.Diagnostics.DiagnosticSource from 10.0.11 to 10.0.12\n\n---\nupdated-dependencies:\n- dependency-name: Meziantou.Analyzer\n  dependency-version: 3.0.234\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.Bcl.AsyncInterfaces\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.Bcl.AsyncInterfaces\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.Extensions.Logging.Abstractions\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.Extensions.Logging.Console\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: Microsoft.SourceLink.GitHub\n  dependency-version: 10.0.401\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n- dependency-name: SonarAnalyzer.CSharp\n  dependency-version: 10.34.0.3385\n  dependency-type: direct:production\n  update-type: version-update:semver-minor\n  dependency-group: dotnet-dependencies\n- dependency-name: System.Diagnostics.DiagnosticSource\n  dependency-version: 10.0.12\n  dependency-type: direct:production\n  update-type: version-update:semver-patch\n  dependency-group: dotnet-dependencies\n...\n\nSigned-off-by: dependabot[bot] <support@github.com>\n\n---------\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>\n\n* release: v0.9.0\n\noptions records for all four stages inherit the Abstractions 0.24 base records; 15 superseded constructors hidden; IsDryRun setters deprecated MINOR bump from v0.8.1.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* fix: RequiresUnreferencedCode on the three multi-stream record constructors; observable ReaderSettings/WriterSettings tests (review on #302)\n\nThe record constructors added in #297 on XmlMultiStreamExtractor\n(streams, options, logger) and XmlMultiStreamLoader (streamFactory /\nbufferWriterFactory, options, logger) reach XmlSerializer like every\nother public constructor but lacked the trim/AOT annotation, so a\ntrimming caller got no IL2026 at those entry points.\n\nXmlOptionsRecordTests gains four facts that observe the nested settings\nthrough behaviour rather than assignment: a one-character\nMaxCharactersInDocument on the record's ReaderSettings fails the read on\nboth extractors, and OmitXmlDeclaration on the record's WriterSettings\nis visible in both loaders' output.\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nSigned-off-by: dependabot[bot] <support@github.com>\nCo-authored-by: Claude Opus 4.8 <noreply@anthropic.com>\nCo-authored-by: dependabot[bot] <49699333+dependabot[bot]@users.noreply.github.com>",
          "timestamp": "2026-09-17T03:01:47Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/ded4a70d14f929469bbd571e078ee3afb1751296"
        },
        "date": 1789618951685,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1339980.79296875,
            "unit": "ns",
            "range": "± 3242.7764919558463"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 591354.8802083334,
            "unit": "ns",
            "range": "± 18950.745673727935"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2527114.4140625,
            "unit": "ns",
            "range": "± 329395.2955005242"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 21137401.114583332,
            "unit": "ns",
            "range": "± 24204.08173902549"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 134769790.25,
            "unit": "ns",
            "range": "± 652306.9095538445"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 64601694.44444445,
            "unit": "ns",
            "range": "± 1002340.5006997001"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 198407039,
            "unit": "ns",
            "range": "± 1697499.797938427"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 2145751418.3333333,
            "unit": "ns",
            "range": "± 19291273.35240692"
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
          "id": "efad7989bfbe5867cde6235e2afe46aa9e71d864",
          "message": "docs: tell contributors how to enable the shipped gitleaks pre-commit hook (#316)\n\n* docs: tell contributors how to enable the shipped gitleaks pre-commit hook\n\nThe template upgrade added .githooks/pre-commit, but git only runs it after\n`git config core.hooksPath .githooks`, and nothing in the repository said so.\nAdds the template's Getting Started step (hook, CLI install, --no-verify).\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n* docs: secret-scan step - Linux install path, \"secrets\" not \"credentials\", comma\n\nReview feedback (repo-template#577 carries the same wording upstream).\n\nCo-Authored-By: Claude Opus 5 <noreply@anthropic.com>\n\n---------\n\nCo-authored-by: Chris Wolfgang <cwolfgan@ptd.net>\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-18T02:23:03Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/efad7989bfbe5867cde6235e2afe46aa9e71d864"
        },
        "date": 1789705302799,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1804248.5989583333,
            "unit": "ns",
            "range": "± 214.15236674065355"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 875304.5065104166,
            "unit": "ns",
            "range": "± 7956.539102552826"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2808226.9270833335,
            "unit": "ns",
            "range": "± 104486.02474114475"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 28547536.270833332,
            "unit": "ns",
            "range": "± 50369.70709888895"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 183251578.11111107,
            "unit": "ns",
            "range": "± 471680.0429241527"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 91402209.5,
            "unit": "ns",
            "range": "± 6332958.252906594"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 285616862.6666667,
            "unit": "ns",
            "range": "± 1510766.5510731079"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3129686404.3333335,
            "unit": "ns",
            "range": "± 16755859.6538543"
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
          "id": "7bd470c369de9f23b30888ee5e46d4854c53817b",
          "message": "docs(pack): add THIRD-PARTY-NOTICES.md and ship it in the package (#283) (#354)\n\nAdds the hand-maintained licence notices for the shipped runtime dependencies (Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource on the down-level targets — all MIT) in the Etl-Csv house format, and packs it unconditionally so a missing file fails `dotnet pack`.\n\nCo-authored-by: Claude Opus 5 <noreply@anthropic.com>",
          "timestamp": "2026-09-19T02:16:16Z",
          "url": "https://github.com/Chris-Wolfgang/ETL-Xml/commit/7bd470c369de9f23b30888ee5e46d4854c53817b"
        },
        "date": 1789791663770,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 1000)",
            "value": 1838564.9752604167,
            "unit": "ns",
            "range": "± 13459.99412720955"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 1000)",
            "value": 879034.4814453125,
            "unit": "ns",
            "range": "± 8305.235639548246"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 1000)",
            "value": 2813045.828125,
            "unit": "ns",
            "range": "± 128775.99051994634"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 1000)",
            "value": 27836379.104166668,
            "unit": "ns",
            "range": "± 45480.56031753787"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Extract(RecordCount: 100000)",
            "value": 188746452.11111107,
            "unit": "ns",
            "range": "± 1473056.3292346476"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.Load(RecordCount: 100000)",
            "value": 88185713.16666667,
            "unit": "ns",
            "range": "± 757918.885480223"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.RoundTrip(RecordCount: 100000)",
            "value": 280470887,
            "unit": "ns",
            "range": "± 843194.6804071999"
          },
          {
            "name": "Wolfgang.Etl.Xml.ShadowWorkloads.XmlShadowWorkloads.ConcurrentExtractors(RecordCount: 100000)",
            "value": 3088628987.6666665,
            "unit": "ns",
            "range": "± 2542199.1191888046"
          }
        ]
      }
    ]
  }
}