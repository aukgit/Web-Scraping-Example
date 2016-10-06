using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = System.Windows.Forms.HtmlDocument;

namespace WebScrappingExample {
    public partial class Form1 : Form {
        private Thread thread;
        private bool threadRunning;


        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            thread = new Thread(() => {
                try {
                    threadRunning = true;
                    this.Invoke(new MethodInvoker(delegate {
                        scapBtn.Enabled = false;
                        AbortBtn.Visible = true;
                        progressBar1.Visible = true;
                    }));

                    HtmlWeb htmlWeb = new HtmlWeb() {
                        AutoDetectEncoding = false,
                        OverrideEncoding = Encoding.GetEncoding("iso-8859-2")
                    };
                    var urlTemplate = UrlTextBox.Text;
                    var notePadList = new List<string>(5000);
                    for (int i = 'A'; i <= 'Z'; i++) {
                        var alphabet = ((char) i).ToString();
                        var parsingUrl = urlTemplate.Replace("%d", alphabet);
                        var htmlDocument = htmlWeb.Load(parsingUrl);
                        var firstOrDefault = htmlDocument.
                            DocumentNode.
                            Descendants("ul").
                            FirstOrDefault(n => n.Attributes.Contains("class") &&
                                                n.Attributes["class"].Value.Equals("companyList"));
                        notePadList.Add(parsingUrl);
                        if (firstOrDefault != null) {
                            var listItems = firstOrDefault
                                .Descendants("li").ToList();

                            this.Invoke(new MethodInvoker(delegate {
                                progressBar1.Value = 0;
                                progressBar1.Maximum = listItems.Count;
                            }));

                            foreach (var item in listItems) {
                                var anchorFound = item.Descendants("a").FirstOrDefault();
                                if (anchorFound != null) {
                                    var companyName = anchorFound.InnerText;
                                    notePadList.Add(companyName);
                                }
                                this.Invoke(new MethodInvoker(delegate {
                                    progressBar1.Value++;
                                }));
                            }
                        }
                    }
                    File.WriteAllLines("Company-list.txt", notePadList);


                    this.Invoke(new MethodInvoker(delegate {
                        MessageBox.Show(@"File has been written at [Company-list.txt] A-Z all companies saved.", @"Running", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        scapBtn.Enabled = true;
                        AbortBtn.Visible = false;
                        progressBar1.Visible = false;
                    }));

                    threadRunning = false;
                    thread.Abort();
                    thread = null;
                } catch (Exception ex) {
                    // ignored
                }
            });
            thread.Start();
        }

        private void AbortBtn_Click(object sender, EventArgs e) {
            if (thread != null) {
                threadRunning = false;
                thread.Abort();
                this.Invoke(new MethodInvoker(delegate {
                    MessageBox.Show(@"Aborting by the user.", @"Running", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    scapBtn.Enabled = true;
                    AbortBtn.Visible = false;
                    progressBar1.Visible = false;
                }));
            }
        }
    }
}
