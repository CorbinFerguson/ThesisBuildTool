using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Reporting
{
    public static class TestReport
    {
        #region Fields
        private static readonly string RunId = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        private static string _reportsRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "L5XFiles", "TestReports"));
        private static bool _rootLocked = false;


        private static readonly AsyncLocal<TestContextState> Current = new AsyncLocal<TestContextState>();
        public static string CurrentTestFolder => Current.Value?.ClassFolder;
        public static string CurrentTestFilePath => Current.Value?.HtmlPath;
        #endregion

        #region Setup
        public static void SetReportsRoot(string absolutePath)
        {
            if (_rootLocked) return;
            _reportsRoot = absolutePath ?? throw new ArgumentNullException("absolutePath");
            Directory.CreateDirectory(_reportsRoot);
        }

        public static void Start([CallerMemberName] string testName = null, string testClassName = null)
        {
            StartInternal(testName, testClassName);
        }

        public static void Start(string testName, string testClassName, string groupOrSuite = null)
        {
            StartInternal(testName, testClassName, groupOrSuite);
        }

        public static void End()
        {
            EnsureStarted();

            var ctx = Current.Value;

            // Compute summary BEFORE footer so we can write to the class index
            var ended = DateTime.Now;
            var duration = ended - ctx.StartedAt;
            var status = ctx.Failures.Count == 0 ? "PASS" : "FAIL";
            var assertCount = ctx.AssertCount;
            var failureCount = ctx.Failures.Count;
            var testFileName = Path.GetFileName(ctx.HtmlPath);

            try
            {
                if (!ctx.FooterWritten)
                {
                    WriteSummaryFooter(ctx);
                    ctx.FooterWritten = true;
                }

                try
                {
                    AppendIndexRowAndRegenerateIndex(
                        ctx.ClassFolder,
                        ctx.TestName,
                        ctx.TestClassName,
                        status,
                        assertCount,
                        failureCount,
                        ctx.StartedAt,
                        duration,
                        testFileName
                    );
                }
                catch
                {
                    // Never let index generation break the test outcome
                }
            }
            finally
            {
                if (ctx.Failures.Count > 0)
                {
                    var msg = "Soft assertion failures (" + ctx.Failures.Count + "):\n - " +
                              string.Join("\n - ", ctx.Failures);
                    Current.Value = null;   // clear before throwing
                    throw new Exception(msg);
                }
                Current.Value = null;
            }
        }

        public static void AssertAll()
        {
            EnsureStarted();

            var ctx = Current.Value;
            if (ctx.Failures.Count > 0)
            {
                if (!ctx.FooterWritten)
                {
                    WriteSummaryFooter(ctx);
                    ctx.FooterWritten = true;
                }
                var msg = "Soft assertion failures (" + ctx.Failures.Count + "):\n - " +
                          string.Join("\n - ", ctx.Failures);
                throw new Exception(msg);
            }
        }

        public static void SetScreenshotProvider(Func<string> capturePathFunc)
        {
            EnsureStarted();
            Current.Value.ScreenshotProvider = capturePathFunc;
        }

        public static void AttachScreenshot(string absoluteOrRelativePath, string caption)
        {
            EnsureStarted();
            var ctx = Current.Value;
            var rel = MakePathRelative(absoluteOrRelativePath, ctx.ClassFolder);
            WriteRow(ctx, "INFO", caption ?? "Screenshot", rel);
        }

        public static void Section(string title)
        {
            EnsureStarted();
            WriteSection(Current.Value, title);
        }

        public static void Info(string message)
        {
            EnsureStarted();
            WriteRow(Current.Value, "INFO", message, null);
        }
        #endregion

        #region Evaluations

        public static void IsTrue(bool condition, string userMessage, bool captureOnFailure = true)
        {
            EnsureStarted();

            var ctx = Current.Value;
            ctx.AssertCount++;

            if (condition)
            {
                WriteRow(ctx, "PASS", userMessage, null);
            }
            else
            {
                string relImg = null;
                if (captureOnFailure && ctx.ScreenshotProvider != null)
                {
                    try
                    {
                        var shotPath = ctx.ScreenshotProvider();
                        if (!string.IsNullOrEmpty(shotPath) && File.Exists(shotPath))
                            relImg = MakePathRelative(shotPath, ctx.ClassFolder);
                    }
                    catch { /* best-effort */ }
                }

                WriteRow(ctx, "FAIL", userMessage, relImg);
                ctx.Failures.Add(userMessage);
            }
        }

        public static void IsFalse(bool condition, string message, bool captureOnFailure = true)
        {
            IsTrue(!condition, message, captureOnFailure);
        }

        public static void IsNotNull(object obj, string message, bool captureOnFailure = true)
        {
            bool notNull = obj is null;
            IsFalse(notNull, message, captureOnFailure);
        }

        public static void IsNull(object obj, string message, bool captureOnFailure = true)
        {
            bool isNull = obj is null;
            IsTrue(isNull, message, captureOnFailure);
        }

        public static void AreEqual<T>(T expected, T actual, string message, bool captureOnFailure = true)
        {
            bool ok = object.Equals(expected, actual);
            IsTrue(ok, message, captureOnFailure);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message, bool captureOnFailure = true)
        {
            bool ok = !object.Equals(notExpected, actual);
            IsTrue(ok, message, captureOnFailure);
        }

        public static void Fail(string message, bool captureScreenshot = true)
        {
            IsTrue(false, message, captureScreenshot);
        }

        private static void StartInternal(string testName, string testClassName, string groupOrSuite = null)
        {
            if (Current.Value != null) return; // already started for this thread
            _rootLocked = true;

            var tn = !string.IsNullOrEmpty(testName) ? testName : AutoDetectTestName();
            var cn = !string.IsNullOrEmpty(testClassName) ? testClassName : AutoDetectTestClass();

            var classSimple = ClassSimpleName(cn);

            var runRoot = Path.Combine(_reportsRoot, RunId);
            Directory.CreateDirectory(runRoot);

            string classFolder;
            if (!string.IsNullOrEmpty(groupOrSuite))
                classFolder = Path.Combine(runRoot, Sanitize(groupOrSuite), Sanitize(classSimple));
            else
                classFolder = Path.Combine(runRoot, Sanitize(classSimple));

            Directory.CreateDirectory(classFolder);

            var htmlFile = Path.Combine(classFolder, Sanitize(tn) + ".html");

            var ctx = new TestContextState
            {
                TestName = tn,
                TestClassName = classSimple,
                ClassFolder = classFolder,
                HtmlPath = htmlFile,
                Failures = new List<string>(),
                WriteLock = new object(),
                AssertCount = 0,
                StartedAt = DateTime.Now,
                FooterWritten = false,
                ScreenshotProvider = null
            };

            Current.Value = ctx;

            WriteHtmlHeader(ctx);
        }

        private static void EnsureStarted()
        {
            if (Current.Value == null)
                throw new InvalidOperationException("TestReport.Start(testName, testClassName) must be called in TestInitialize before using TestReport.*");
        }
        #endregion

        #region HTML_Write

        private static void WriteHtmlHeader(TestContextState ctx)
        {
            lock (ctx.WriteLock)
            {
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
                sb.AppendLine("<title>Test Report - " + Html(ctx.TestClassName + "." + ctx.TestName) + "</title>");
                sb.AppendLine(
@"<style>
body { font-family: Segoe UI, Roboto, Arial, sans-serif; margin: 16px; }
h1 { margin: 0 0 8px 0; }
.meta { color:#555; margin-bottom: 16px; }
.section { margin: 16px 0 8px 0; padding: 6px 10px; background:#eef6ff; border-left: 4px solid #1976d2; font-weight:600; }
.row { border-bottom: 1px solid #eee; padding: 8px 4px; display:flex; gap:12px; align-items:flex-start; }
.tag { min-width:52px; padding:2px 6px; border-radius:4px; color:#fff; font-weight:600; font-size:12px; text-align:center; }
.PASS { background:#2e7d32; }
.FAIL { background:#d32f2f; }
.INFO { background:#616161; }
.time { color:#777; font-size:12px; min-width:110px; }
.msg { white-space: pre-wrap; }
.kv { color:#444; font-size:12px; }
img { max-width: 600px; border:1px solid #ddd; margin-top:6px; }
.summary { margin-top: 16px; padding-top: 8px; border-top: 2px solid #ddd; }
</style></head><body>");
                sb.AppendLine("<h1>" + Html(ctx.TestClassName + "." + ctx.TestName) + "</h1>");
                sb.AppendLine("<div class='meta'>Started: " + Html(ctx.StartedAt.ToString("u")) + "</div>");
                File.WriteAllText(ctx.HtmlPath, sb.ToString());
            }
        }

        private static void WriteSection(TestContextState ctx, string title)
        {
            lock (ctx.WriteLock)
            {
                File.AppendAllText(ctx.HtmlPath, "<div class='section'>" + Html(title) + "</div>\n");
            }
        }

        private static void WriteRow(TestContextState ctx, string level, string message, string relPathImage)
        {
            var now = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
            var sb = new StringBuilder();
            sb.Append("<div class='row'>");
            sb.Append("<div class='tag " + level + "'>" + level + "</div>");
            sb.Append("<div class='time'>" + Html(now) + "</div>");
            sb.Append("<div class='msg'>");
            sb.Append(Html(message ?? ""));
            if (!string.IsNullOrEmpty(relPathImage))
            {
                var cap = Path.GetFileName(relPathImage);
                sb.Append("<div class='kv'>Screenshot: " + Html(cap) + "</div>");
                sb.Append("<a href='" + Html(relPathImage) + "' target='_blank'>");
                sb.Append("<img src='" + Html(relPathImage) + "' alt='screenshot' />");
                sb.Append("</a>");
            }
            sb.Append("</div></div>\n");

            lock (ctx.WriteLock)
            {
                File.AppendAllText(ctx.HtmlPath, sb.ToString());
            }
        }

        private static void WriteSummaryFooter(TestContextState ctx)
        {
            var ended = DateTime.Now;
            var dur = ended - ctx.StartedAt;
            var sb = new StringBuilder();
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine("Ended: " + Html(ended.ToString("u")) + "<br/>");
            sb.AppendLine("Assertions: " + ctx.AssertCount + "<br/>");
            sb.AppendLine("Failures: " + ctx.Failures.Count + "<br/>");
            sb.AppendLine("Duration: " + dur.ToString(@"hh\:mm\:ss\.fff") + "<br/>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            lock (ctx.WriteLock)
            {
                File.AppendAllText(ctx.HtmlPath, sb.ToString());
            }
        }

        private static void AppendIndexRowAndRegenerateIndex(
            string classFolder,
            string testName,
            string testClassName,
            string status,
            int assertCount,
            int failureCount,
            DateTime startedAt,
            TimeSpan duration,
            string reportFileName)
        {
            ConcurrentDictionary<string, object> _classFolderLocks = new ConcurrentDictionary<string, object>();
            var lockObj = _classFolderLocks.GetOrAdd(classFolder, _ => new object());

            lock (lockObj)
            {
                Directory.CreateDirectory(classFolder);

                var tsvPath = Path.Combine(classFolder, ".index.tsv");
                var htmlPath = Path.Combine(classFolder, "ReportIndex.html");

                // Append one TSV row: TestName\tStatus\tAssertions\tFailures\tStartedUtc\tDuration\tReportFile
                var row = string.Join("\t", new[]
                {
                    testName ?? "",
                    status ?? "",
                    assertCount.ToString(CultureInfo.InvariantCulture),
                    failureCount.ToString(CultureInfo.InvariantCulture),
                    startedAt.ToUniversalTime().ToString("u").TrimEnd('Z'), // ISO-like UTC
                    duration.ToString(@"hh\:mm\:ss\.fff"),
                    reportFileName ?? ""
                }) + Environment.NewLine;

                File.AppendAllText(tsvPath, row);

                // Rebuild HTML index from TSV
                var lines = File.ReadAllLines(tsvPath);

                var pass = 0; var fail = 0;
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
                sb.AppendLine("<title>" + Html(testClassName) + " - Report Index</title>");
                sb.AppendLine(@"<style>
body { font-family: Segoe UI, Roboto, Arial, sans-serif; margin: 16px; }
h1 { margin: 0 0 12px 0; }
.meta { color:#555; margin-bottom: 16px; }
table { border-collapse: collapse; width: 100%; }
th, td { border: 1px solid #eee; padding: 8px 10px; text-align: left; }
th { background:#f7f7f7; }
.badge { display:inline-block; min-width:48px; padding:2px 6px; border-radius:4px; color:#fff; font-weight:600; font-size:12px; text-align:center; }
.badge.PASS { background:#2e7d32; }
.badge.FAIL { background:#d32f2f; }
tfoot td { font-weight:600; }
.small { color:#666; font-size:12px; }
</style></head><body>");
                sb.AppendLine("<h1>" + Html(testClassName) + " – Report Index</h1>");
                sb.AppendLine("<div class='meta'>Run Id: " + Html(RunId) + " • Generated: " + Html(DateTime.Now.ToString("u")) + "</div>");

                sb.AppendLine("<table>");
                sb.AppendLine("<thead><tr>" +
                              "<th>#</th><th>Test</th><th>Status</th><th>Assertions</th><th>Failures</th>" +
                              "<th>Started (UTC)</th><th>Duration</th><th>Report</th></tr></thead>");
                sb.AppendLine("<tbody>");

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('\t');
                    if (parts.Length < 7) continue;

                    var tName = parts[0];
                    var st = parts[1];
                    var asrt = parts[2];
                    var fl = parts[3];
                    var stUtc = parts[4];
                    var dur = parts[5];
                    var file = parts[6];

                    if (string.Equals(st, "PASS", StringComparison.OrdinalIgnoreCase)) pass++;
                    else fail++;

                    sb.Append("<tr>");
                    sb.Append("<td>" + (i + 1).ToString(CultureInfo.InvariantCulture) + "</td>");
                    sb.Append("<td>" + Html(tName) + "</td>");
                    sb.Append("<td><span class='badge " + Html(st) + "'>" + Html(st) + "</span></td>");
                    sb.Append("<td>" + Html(asrt) + "</td>");
                    sb.Append("<td>" + Html(fl) + "</td>");
                    sb.Append("<td class='small'>" + Html(stUtc) + "</td>");
                    sb.Append("<td class='small'>" + Html(dur) + "</td>");
                    sb.Append("<td><a href='" + Html(file) + "'>Open report</a></td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("<tfoot><tr>" +
                              "<td colspan='2'>Totals</td>" +
                              "<td>" + (pass + fail) + " tests</td>" +
                              "<td colspan='1'>Assertions—</td>" +
                              "<td>" + fail + " failed</td>" +
                              "<td colspan='3'>Pass: " + pass + " • Fail: " + fail + "</td>" +
                              "</tr></tfoot>");
                sb.AppendLine("</table>");
                sb.AppendLine("</body></html>");

                File.WriteAllText(htmlPath, sb.ToString());
            }
        }
        #endregion

        #region Helpers
        private static string ClassSimpleName(string fullOrSimple)
        {
            if (string.IsNullOrEmpty(fullOrSimple)) return "UnknownClass";
            var idx = fullOrSimple.LastIndexOf('.');
            return idx >= 0 ? fullOrSimple.Substring(idx + 1) : fullOrSimple;
        }

        private static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Test";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private static string AutoDetectTestName()
        {
            var rn = typeof(TestReport).FullName ?? "";
            var st = new StackTrace();
            for (int i = 1; i < st.FrameCount; i++)
            {
                var m = st.GetFrame(i)?.GetMethod();
                var t = m?.DeclaringType;
                var full = t?.FullName;
                if (full != null && full.IndexOf(rn, StringComparison.Ordinal) < 0)
                    return (m != null ? m.Name : "TestMethod");
            }
            return "Test_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        private static string AutoDetectTestClass()
        {
            var rn = typeof(TestReport).FullName ?? "";
            var st = new StackTrace();
            for (int i = 1; i < st.FrameCount; i++)
            {
                var m = st.GetFrame(i)?.GetMethod();
                var t = m?.DeclaringType;
                var full = t?.FullName;
                if (full != null && full.IndexOf(rn, StringComparison.Ordinal) < 0)
                    return t != null ? t.FullName : "TestClass";
            }
            return "TestClass";
        }

        private static string Html(string s)
        {
            if (s == null) return "";
            return System.Net.WebUtility.HtmlEncode(s);
        }

        private static string MakePathRelative(string path, string baseFolder)
        {
            try
            {
                var full = Path.GetFullPath(path);
                var baseFull = Path.GetFullPath(baseFolder);
                var uri = new Uri(full);
                var baseUri = new Uri(baseFull + Path.DirectorySeparatorChar);
                var rel = baseUri.MakeRelativeUri(uri).ToString().Replace('/', Path.DirectorySeparatorChar);
                return Uri.UnescapeDataString(rel);
            }
            catch { return path; }
        }

        private sealed class TestContextState
        {
            public string TestName;
            public string TestClassName;
            public string ClassFolder;
            public string HtmlPath;
            public List<string> Failures;
            public object WriteLock;
            public int AssertCount;
            public DateTime StartedAt;
            public bool FooterWritten;
            public Func<string> ScreenshotProvider;
        }
        #endregion
    }
}