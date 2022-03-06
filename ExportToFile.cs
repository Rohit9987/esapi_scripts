using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.CA.Scripting;
using System.IO;
using System.Threading.Tasks;

namespace VMS.DV.PD.Scripting {

  public class Script {

    public Script() {
            
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/) {
            // TODO : Add here your code that is called when the script is launched from Portal Dosimetry

            List<string> textoutput = new List<string>();


            #region patientDetails
            var patient = context.Patient;
            string name = patient.Name;
            string firstname = patient.FirstName;
            //MessageBox.Show("Name: " + firstname + name);
            textoutput.Add("Name," + firstname +"_"+ name);

            string id = patient.Id;
            //MessageBox.Show("Id," + id);
            textoutput.Add("Id," + id);

            string planName = context.PDPlanSetup.Id;
            //MessageBox.Show("Plan name," + planName);
            textoutput.Add("Plan_name," + planName);

            var beamtype = context.PDPlanSetup.Beams.FirstOrDefault().Beam.MLCPlanType;
            //MessageBox.Show("Technique : "+beamtype.ToString());
            textoutput.Add("Technique," + beamtype.ToString());
            #endregion

            // retrieve composite gamma and evaluation parameters
            var compositeImageAnalysis = context.PDPlanSetup.CompositeImages.LastOrDefault().Analyses.LastOrDefault();

            string distta = compositeImageAnalysis.GammaParamDTA.ToString();
            string doseta = ((compositeImageAnalysis.GammaParamDoseDiff) * 100).ToString();
            //MessageBox.Show("Gamma(" + distta + "mm, " + doseta + "%)");
            textoutput.Add("Field,Gamma(" + distta + "mm_" + doseta + "%)");


            // retrieve the gamma for each beam.         - need to get (3mm, 3%). available in analysis.
            #region individual beam gamma
            var beams = context.PDPlanSetup.Beams;
            
            foreach(var beam in beams)
            {
                var analysis = beam.PortalDoseImages.First().Analyses.LastOrDefault();

                ImageRT image = analysis.GammaImage;
                
                var roiMask = analysis.ROIMask;

                if (image != null)
                {
                    ImageRT gammaImage = image;
                    if (gammaImage != null)
                    {
                        Frame gammaFrame = gammaImage.Frames[0];
                        ushort[,] pixelsGamma = new ushort[gammaFrame.XSize, gammaFrame.YSize];
                        gammaFrame.GetVoxels(0, pixelsGamma);

                        int pixelsinROI = 0;
                        int pixelsInROIGammaLessOne = 0;
                        int pixelsinGammaLessOne = 0;

                        for (int y = 0; y < gammaFrame.YSize; y++)
                        {
                            for (int x = 0; x < gammaFrame.XSize; x++)
                            {
                                bool isGammaLessOne = gammaFrame.VoxelToDisplayValue(pixelsGamma[x, y]) < 1.0;
                                if (isGammaLessOne)
                                    pixelsinGammaLessOne++;
                                if (roiMask[x, y] != 0)
                                {
                                    pixelsinROI++;
                                    if (isGammaLessOne)
                                        pixelsInROIGammaLessOne++;
                                }
                            }
                        }

                        double gammaLessthanone = pixelsInROIGammaLessOne;
                        double roipixels = pixelsinROI;
                        double gammaPassRate = gammaLessthanone / roipixels * 100;
                       // MessageBox.Show(beam.Id+" Gamma Pass Rate : " + Math.Round(gammaPassRate,2));
                        textoutput.Add(beam.Id + "," + Math.Round(gammaPassRate, 2));
                    }

                }  
            }
            #endregion

            // retrive gamma for each composite
            #region compositegamma
            

            ImageRT CIimage = compositeImageAnalysis.GammaImage;
            var CIroiMask = compositeImageAnalysis.ROIMask;
            if (CIimage != null)
            {
                ImageRT gammaImage = CIimage;
                if (gammaImage != null)
                {
                    Frame gammaFrame = gammaImage.Frames[0];
                    ushort[,] pixelsGamma = new ushort[gammaFrame.XSize, gammaFrame.YSize];
                    gammaFrame.GetVoxels(0, pixelsGamma);

                    int pixelsinROI = 0;
                    int pixelsInROIGammaLessOne = 0;
                    int pixelsinGammaLessOne = 0;

                    for (int y = 0; y < gammaFrame.YSize; y++)
                    {
                        for (int x = 0; x < gammaFrame.XSize; x++)
                        {
                            bool isGammaLessOne = gammaFrame.VoxelToDisplayValue(pixelsGamma[x, y]) < 1.0;
                            if (isGammaLessOne)
                                pixelsinGammaLessOne++;
                            if (CIroiMask[x, y] != 0)
                            {
                                pixelsinROI++;
                                if (isGammaLessOne)
                                    pixelsInROIGammaLessOne++;
                            }
                        }
                    }

                    double gammaLessthanone = pixelsInROIGammaLessOne;
                    double roipixels = pixelsinROI;
                    double gammaPassRate = gammaLessthanone / roipixels * 100;
                    //MessageBox.Show("Composite Gamma Pass Rate : " + Math.Round(gammaPassRate, 2));
                    textoutput.Add("Composite," + Math.Round(gammaPassRate, 2));
                }
            }
            #endregion
            StreamWriterOne.ExampleAsync(textoutput);

        }
    }

    class StreamWriterOne
    {
        public static void ExampleAsync(List<string> textoutput)
        {
            string name = textoutput[0];
            name = name.Replace('-', '_');
            name = name.Split(',')[1];
            
            
            StreamWriter file = new StreamWriter("D:\\" + name+".csv");
            foreach (string line in textoutput)
            {
                //MessageBox.Show(line);
                file.WriteLine(line);
            }
            file.Flush();
            file.Close();

            MessageBox.Show("Saved file to D drive.");
        }


    } 
}