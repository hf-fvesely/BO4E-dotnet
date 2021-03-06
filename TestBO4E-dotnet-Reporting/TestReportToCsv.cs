﻿using BO4E.Reporting;

using Itenso.TimePeriod;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestBO4E.Reporting
{
    [TestClass]
    public class TestReportToCsv
    {
        [TestMethod]
        public void TestCompletenessReportToCsv()
        {
            CompletenessReport cr = new CompletenessReport()
            {
                LokationsId = "DE12345",
                Coverage = 0.87M, // 87%
                wertermittlungsverfahren = BO4E.ENUM.Wertermittlungsverfahren.PROGNOSE,
                ReferenceTimeFrame = new BO4E.COM.Zeitraum()
                {
                    Startdatum = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Enddatum = new DateTimeOffset(2019, 3, 1, 0, 0, 0, TimeSpan.Zero)
                },
            };
            string result = cr.ToCsv(';', true, Environment.NewLine);
            var lines = new List<string>(result.Split(Environment.NewLine));
            Assert.AreEqual(2, lines.Count);

            // reihenfolge
            List<Dictionary<string, string>> reihenfolge = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>() { ["LokationsId"] = "messlokationsId" },
                new Dictionary<string, string>() { ["Coverage"] = "Newcoverage" },
                new Dictionary<string, string>() { ["Zeitraum.Startdatum"] = "time.startdatum" },
                new Dictionary<string, string>() { ["Zeitraum.Enddatum"] = "time.enddatum" }
            };

            //string JSONdata = "{'completenessZfa':[{'lokationsId':'lokationsId'},{'coverage':'coverage'},{'Zeitraum.einheit':'einheit'},{'Zeitraum.dauer':'dauer'},{'Zeitraum.startdatum':'startdatum'},{'Zeitraum.enddatum':'enddatum'},{'obiskennzahl':'obiskennzahl'},{'einheit':'einheit'},{'wertermittlungsverfahren':'wertermittlungsverfahren'},{'startdatum':'Verbrauch.startdatum'},{'enddatum':'Verbrauch.enddatum'},{'wert':'Verbrauch.wert'},{'headerLine':'1'}]}";
            //var alldata = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(JSONdata);
            //List<Dictionary<string, string>> reihenfolge = alldata["completenessZfa"];

            var Newresult = cr.ToCsv(';', true, Environment.NewLine, reihenfolge);
            lines = new List<string>(Newresult.Split(Environment.NewLine));
            Assert.AreEqual(2, lines.Count);
            string decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual("DE12345;0" + decimalSeparator + "87;2019-01-01T00:00:00Z;2019-03-01T00:00:00Z;", lines[1]);
            var commaResult = cr.ToCsv(',', lineTerminator: Environment.NewLine, reihenfolge: reihenfolge);
            Assert.AreEqual("DE12345,0" + decimalSeparator + "87,2019-01-01T00:00:00Z,2019-03-01T00:00:00Z,", commaResult.Split(Environment.NewLine)[1]);
            var dpunktResult = cr.ToCsv(':', lineTerminator: Environment.NewLine, reihenfolge: reihenfolge);
            Assert.AreEqual("DE12345:0" + decimalSeparator + "87:\"2019-01-01T00:00:00Z\":\"2019-03-01T00:00:00Z\":", dpunktResult.Split(Environment.NewLine)[1]);

            cr.Values = new List<CompletenessReport.BasicVerbrauch>
            {
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 17,
                    Startdatum = new DateTimeOffset(2019,1,1,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,2,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 21,
                    Startdatum = new DateTimeOffset(2019,1,7,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,8,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 35,
                    Startdatum = new DateTimeOffset(2019,1,12,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,13,0,0,0, TimeSpan.Zero).UtcDateTime
                }
            };

            reihenfolge.Add(new Dictionary<string, string>() { ["Wert"] = "V.wert" });
            reihenfolge.Add(new Dictionary<string, string>() { ["Startdatum"] = "V.startdatum" });
            reihenfolge.Add(new Dictionary<string, string>() { ["Enddatum"] = "V.enddatum" });

            var multiplicityResult = cr.ToCsv(lineTerminator: Environment.NewLine, reihenfolge: reihenfolge);
            Assert.AreEqual(2 + cr.Values.Count, new List<string>(multiplicityResult.Split(Environment.NewLine)).Count);
        }
        [TestMethod]
        public void TestDeserialisationCompletenessReportColumnsToCsv()
        {
            string jsonData = "{'completenessZfa':[{'lokationsId':'lokationsId'},{'coverage':'coverage'},{'referenceTimeFrame.einheit':'referenceTimeFrame.einheit'},{'referenceTimeFrame.dauer':'referenceTimeFrame.dauer'},{'referenceTimeFrame.startdatum':'referenceTimeFrame.startdatum'},{'referenceTimeFrame.enddatum':'referenceTimeFrame.enddatum'},{'obiskennzahl':'obiskennzahl'},{'einheit':'einheit'},{'wertermittlungsverfahren':'wertermittlungsverfahren'},{'values.startdatum':'Verbrauch.startdatum'},{'values.enddatum':'Verbrauch.enddatum'},{'values.wert':'Verbrauch.wert'},{'headerLine':'1'}]}";
            var Reihenfolge = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(jsonData);
            Assert.AreEqual("coverage", Reihenfolge["completenessZfa"][1].Values.First());
        }

        [TestMethod]
        public void TestDeserialisationCompletenessReportToCsv()
        {
            string jsonData = "[{\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":\"Text of Message\",\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"}]";
            var reports = JsonConvert.DeserializeObject<List<CompletenessReport>>(jsonData);
            string lastCsvText = string.Empty;
            int counter = 1;
            foreach (var report in reports)
            {
                lastCsvText += report.ToCsv(';', (counter == 1 ? true : false), Environment.NewLine, null) + Environment.NewLine;
                counter++;
            }
            Assert.IsTrue(lastCsvText.Length > 0);
        }

        [TestMethod]
        public void TestPrivateFieldsAndUserProperties()
        {
            string jsonData = "[{\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"}]";
            var reports = JsonConvert.DeserializeObject<List<CompletenessReport>>(jsonData);
            string lastCsvText = string.Empty;
            int counter = 1;
            foreach (var report in reports)
            {
                lastCsvText += report.ToCsv(';', (counter == 1), Environment.NewLine, null) + Environment.NewLine;
                counter++;
            }
            Assert.IsTrue(lastCsvText.Length > 0);
            Assert.IsFalse(lastCsvText.Contains(BO4E.BO.BusinessObject.USER_PROPERTIES_NAME));
            Assert.IsFalse(lastCsvText.Contains("_errorMessage"));
        }

        [TestMethod]
        public void TestErrorCsv()
        {
            string jsonData = "{\"boTyp\":\"COMPLETENESSREPORT\",\"versionStruktur\":1,\"obiskennzahl\":\"1-1:5.29.0\",\"values\":[],\"einheit\":\"ZERO\",\"gaps\":null,\"referenceTimeFrame\":{\"einheit\":\"TAG\",\"dauer\":1,\"startdatum\":\"2020-06-30T22:00:00+00:00\",\"enddatum\":\"2020-07-01T22:00:00+00:00\"},\"wertermittlungsverfahren\":\"PROGNOSE\",\"lokationsId\":\"99998888777\",\"coverage\":null,\"profil\":\"000000000123456789\",\"profilRolle\":\"0002\",\"anlagennummer\":\"1234567890\",\"zw\":\"000000000020707999\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"coverage_sum\":0}";
            var report = JsonConvert.DeserializeObject<CompletenessReport>(jsonData);
            var csv = report.ToCSV();
            Assert.IsTrue(csv.Contains("99998888777"));
        }

        [TestMethod]
        public void TestPrivateFieldsAndUserPropertiesHardCodingCSV()
        {
            string jsonData = "[{\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-65:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-09-30T22:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.8402684564,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-27T00:00:00Z\",\"enddatum\":\"2019-10-31T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"50985149762\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5000080657\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"},{\"$type\":\"BO4E.Reporting.CompletenessReport, BO4Enet\",\"versionStruktur\":1,\"boTyp\":\"COMPLETENESSREPORT\",\"referenceTimeFrame\":{\"$type\":\"BO4E.COM.Zeitraum, BO4Enet\",\"einheit\":null,\"dauer\":null,\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\"},\"_errorMessage\":null,\"lokationsId\":\"DE0004096816100000000000000200712\",\"obiskennzahl\":\"1-1:1.29.0\",\"einheit\":2000,\"wertermittlungsverfahren\":1,\"coverage\":0.3,\"values\":[],\"gaps\":[{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-10-31T23:00:00Z\",\"enddatum\":\"2019-11-13T00:00:00Z\",\"wert\":null},{\"$type\":\"BO4E.Reporting.CompletenessReport+BasicVerbrauch, BO4Enet\",\"startdatum\":\"2019-11-22T00:00:00Z\",\"enddatum\":\"2019-11-30T23:00:00Z\",\"wert\":null}],\"profil\":\"000000000111127365\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5001065966\",\"zw\":\"000000000020708905\",\"sap_time_zone\":\"CET\",\"sapSanitized\":true,\"valueCount\":0,\"overallGapStart\":\"2019-10-31T23:00:00Z\",\"overallGapEnd\":\"2019-11-30T23:00:00Z\"}]";
            var reports = JsonConvert.DeserializeObject<List<CompletenessReport>>(jsonData);
            string lastCsvText = string.Empty;
            int counter = 0;
            foreach (var report in reports)
            {
                string singleReportLine = report.ToCSV(";", (counter == 0), Environment.NewLine) + Environment.NewLine;
                if (counter == 0)
                {
                    Assert.IsTrue(singleReportLine.Split(Environment.NewLine)[1].StartsWith("2019-09-30T22:00:00Z;2019-10-31T23:00:00Z;;50985149762")); // no melo, just malo
                    Assert.IsTrue(singleReportLine.Contains("IMS"));
                    var missingEntries = ((new DateTime(2019,10,31,23,0,0,0,DateTimeKind.Utc) - new DateTime(2019, 10, 27, 0, 0, 0, 0, DateTimeKind.Utc)).TotalHours * 4).ToString();
                    Assert.IsTrue(singleReportLine.Contains($";{missingEntries};"));
                }
                else if (counter == 0)
                {
                    Assert.IsTrue(singleReportLine.StartsWith("2019-09-30T22:00:00Z;2019-10-31T23:00:00Z;DE0004096816100000000000000200712;;"));// no malo, just melo
                    Assert.IsTrue(singleReportLine.Contains("RLM"));
                }
                lastCsvText += singleReportLine;
                counter++;
            }
            Assert.IsTrue(lastCsvText.Length > 0);
            Assert.IsFalse(lastCsvText.Contains(BO4E.BO.BusinessObject.USER_PROPERTIES_NAME));
            Assert.IsFalse(lastCsvText.Contains("_errorMessage"));

        }


        [TestMethod]
        public void TestCompletenessReportMitGapToCsv()
        {
            CompletenessReport cr = new CompletenessReport()
            {
                LokationsId = "DE12345",
                Coverage = 0.87M, // 87%
                wertermittlungsverfahren = BO4E.ENUM.Wertermittlungsverfahren.PROGNOSE,
                ReferenceTimeFrame = new BO4E.COM.Zeitraum()
                {
                    Startdatum = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Enddatum = new DateTimeOffset(2019, 3, 1, 0, 0, 0, TimeSpan.Zero)
                },
            };
            cr.Values = new List<CompletenessReport.BasicVerbrauch>
            {
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 17,
                    Startdatum = new DateTimeOffset(2019,1,1,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,2,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 21,
                    Startdatum = new DateTimeOffset(2019,1,7,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,8,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 35,
                    Startdatum = new DateTimeOffset(2019,1,12,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2019,1,13,0,0,0, TimeSpan.Zero).UtcDateTime
                }
            };
            cr.Gaps = new List<CompletenessReport.BasicVerbrauch>
            {
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 0,
                    Startdatum = new DateTimeOffset(2017,1,1,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2017,1,2,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 0,
                    Startdatum = new DateTimeOffset(2017,1,7,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2017,1,8,0,0,0, TimeSpan.Zero).UtcDateTime
                },
                new CompletenessReport.BasicVerbrauch()
                {
                    Wert = 0,
                    Startdatum = new DateTimeOffset(2017,1,12,0,0,0, TimeSpan.Zero).UtcDateTime,
                    Enddatum = new DateTimeOffset(2017,1,13,0,0,0, TimeSpan.Zero).UtcDateTime
                }
            };
            var multiplicityResult = cr.ToCsv(lineTerminator: Environment.NewLine);
            Assert.AreEqual(2 + cr.Values.Count + cr.Gaps.Count, new List<string>(multiplicityResult.Split(Environment.NewLine)).Count);
        }

        [TestMethod]
        public void TestCompletenessReportToCsvExceptions()
        {
            CompletenessReport cr = new CompletenessReport()
            {
                LokationsId = "DE12345",
                Coverage = 0.87M, // 87%
                wertermittlungsverfahren = BO4E.ENUM.Wertermittlungsverfahren.PROGNOSE,
                ReferenceTimeFrame = new BO4E.COM.Zeitraum()
                {
                    Startdatum = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Enddatum = new DateTimeOffset(2019, 3, 1, 0, 0, 0, TimeSpan.Zero)
                },
            };

            // reihenfolge
            List<Dictionary<string, string>> reihenfolge = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>() { ["LokationsId"] = "messlokationsId" },
                new Dictionary<string, string>() { ["Coverage"] = "Newcoverage" },
                new Dictionary<string, string>() { ["Zeitraum.Startdatum"] = "time.startdatum" },
                new Dictionary<string, string>() { ["Zeitraum.Enddatum"] = "time.enddatum" },
                new Dictionary<string, string>() { ["Wert"] = null },
                new Dictionary<string, string>() { ["Startdatum"] = "V.startdatum" },
                new Dictionary<string, string>() { ["Enddatum"] = "V.enddatum" },
                null
            };
            string newResult = string.Empty;
            Assert.ThrowsException<ArgumentNullException>(() => cr.ToCsv(';', true, Environment.NewLine, reihenfolge));
            Assert.AreEqual(newResult, "");


            // reihenfolge
            List<Dictionary<string, string>> reihenfolge2 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>() { ["lokationsId"] = "messlokationsId" },
                new Dictionary<string, string>() { ["coverage"] = "Newcoverage" },
                new Dictionary<string, string>() { ["Zeitraum.startdatum"] = "time.startdatum" },
                new Dictionary<string, string>() { ["Zeitraum.enddatum"] = "time.enddatum" },
                new Dictionary<string, string>() { ["wert"] = "V.wert" },
                new Dictionary<string, string>() { ["startdatum"] = "V.startdatum" },
                new Dictionary<string, string>() { ["enddatum"] = "V.enddatum" },
                new Dictionary<string, string>() { ["asdasd"] = "000" }
            };
            Assert.ThrowsException<ArgumentException>(() => cr.ToCsv(';', true, Environment.NewLine, reihenfolge2));
            Assert.AreEqual(newResult, "");
        }

        [TestMethod]
        public void TestSerializingCrWithoutGaps()
        {
            var report = JsonConvert.DeserializeObject<CompletenessReport>("{\"_errorMessage\":\"Cannot use autoconfigured method because there are no values.\",\"boTyp\":\"COMPLETENESSREPORT\",\"versionStruktur\":1,\"obiskennzahl\":\"1-1:1.29.0\",\"values\":[],\"einheit\":0,\"gaps\":null,\"referenceTimeFrame\":{ \"einheit\":4,\"dauer\":1.0,\"startdatum\":\"2020-06-30T22:00:00+00:00\",\"enddatum\":\"2020-07-01T22:00:00+00:00\"},\"wertermittlungsverfahren\":0,\"lokationsId\":\"DE000XXXXXXXXXXXXXXXXXXXXXXXXXXXX\",\"coverage\":0.0,\"profil\":\"000000000111129993\",\"profilRolle\":\"0001\",\"anlagennummer\":\"5111111111\",\"zw\":\"000000000020709888\",\"sap_time_zone\":\"CET\",\"sap_profdecimals\":\"06\",\"sapSanitized\":true,\"valueCount\":0,\"coverage_07-01\":0,\"coverage_07-02\":0,\"coverage_07-03\":0,\"coverage_07-04\":0,\"coverage_07-05\":0,\"coverage_07-06\":0,\"coverage_07-07\":0,\"coverage_07-08\":0,\"coverage_07-09\":0,\"coverage_07-10\":0,\"coverage_07-11\":0,\"coverage_07-12\":0,\"coverage_07-13\":0,\"coverage_07-14\":0,\"coverage_07-15\":0,\"coverage_07-16\":0,\"coverage_07-17\":0,\"coverage_07-18\":0,\"coverage_07-19\":0,\"coverage_07-20\":0,\"coverage_07-21\":0,\"coverage_07-22\":0,\"coverage_07-23\":0,\"coverage_07-24\":0,\"coverage_07-25\":0,\"coverage_07-26\":0,\"coverage_07-27\":0,\"coverage_07-28\":0,\"coverage_07-29\":0,\"coverage_07-30\":0,\"coverage_07-31\":0,\"coverage_sum\":0}");
            report.ToCSV(";", true, Environment.NewLine);
        }
    }
}
