using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimingExtensions
{
    class RunPollingService
    {
        private String url;
        private String currentRun = "";
        private Dictionary<String, List<float>> times = new Dictionary<string, List<float>>();
        private float currentNewTime = 0;

        public RunPollingService(String url, TimeSpan interval)
        {
            this.url = url;
            Timer timer = new Timer((e) =>
            {
                poll();
            },null, TimeSpan.Zero, interval);
        }

        private void poll()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                currentRun = readStream.ReadToEnd();
                

                response.Close();
                readStream.Close();


                currentRun = currentRun.Substring(currentRun.IndexOf("<pre>") + 5, currentRun.IndexOf("</pre>") - currentRun.IndexOf("<pre>"));
                parseRunData(currentRun);
            }

        }

        public String getCurrentRun()
        {
            return "Latest Run time: " + currentNewTime;
        }
        

        private void parseRunData(String rundata)
        {
            StringReader read = new StringReader(rundata);
            read.ReadLine(); //First Header Line
            read.ReadLine(); //Column Headers
            while(read.Peek() != -1)
            {
                //read the data of the car/driver/etc, only need the car number for now
                String driverData = read.ReadLine();
                if (driverData == null)
                {
                    break;
                }
                String carNum = driverData.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First(); //car number is first
                List<float> driverTimes = new List<float>();
                String setData = read.ReadLine();
                if(setData == null)
                {
                    break;
                }
                setData = setData.Replace('\t', ' ');
                setData = setData.Trim(' ');
                while(setData.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First() != "Total") //total line indicates the end of a driver's record
                {
                    List<String> timesStrings = new List<String>(setData.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                    timesStrings.RemoveAt(0); //pop first 4 indicating sets/days/etc
                    timesStrings.RemoveAt(0);
                    timesStrings.RemoveAt(0);
                    timesStrings.RemoveAt(0);
                    foreach(string time in timesStrings)
                    {
                        if (time.StartsWith("("))
                        {
                            driverTimes.Add(float.Parse(time.Trim('(', ')'), CultureInfo.InvariantCulture.NumberFormat));
                        }
                        else
                        {
                            driverTimes.Add(float.Parse(time, CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                    setData = read.ReadLine();
                    setData = setData.Replace('\t', ' ');
                    setData = setData.Trim(' ');
                }
                //done reading in the data now, we can set it into the dict
                
                float newTime = checkForNewRun(carNum, driverTimes);
                if (newTime != 0)
                {
                    this.currentNewTime = newTime; //set the current "new" time
                }
                this.times[carNum] = driverTimes;
            }
        }

        private float checkForNewRun(String carNum, List<float> newTimes)
        {
            if (!times.ContainsKey(carNum)) //no existing times, just return 0
            {
                return 0;
            }
            else if (times[carNum].Count() == newTimes.Count()) //for now just check the count
            {
                return 0;
            }
            else
            {
                return newTimes.Last();
            }
        }
    }
}
