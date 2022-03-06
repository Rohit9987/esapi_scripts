using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.5")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
 [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            Patient patient = context.Patient;

            if(patient == null)
            {
                throw new ApplicationException("Please load a patient.");
            }

            ExternalPlanSetup plan = context.ExternalPlanSetup;

            if(plan == null)
            {
                throw new ApplicationException("Please load a plan");
            }

            patient.BeginModifications();

            VVector isocenter = plan.Beams.FirstOrDefault().IsocenterPosition;
            ExternalBeamTreatmentUnit unit = plan.Beams.FirstOrDefault().TreatmentUnit;

            String unitName = unit.ToString();
            if (unitName.Contains("ExacTrac"))
                unitName = "TrueBeamExacTrac";
            else if (unitName.Contains("Clarity"))
                unitName = "TrueBeamClarity";
            else
            {
                MessageBox.Show("Unknown Machine");
                return;

            }
                
             ExternalBeamMachineParameters machineParameters =
                new ExternalBeamMachineParameters(unitName, "6X", 600, "STATIC", string.Empty);
            

            //Add setup fields
            Beam setup_ant = plan.AddSetupBeam(machineParameters, new VRect<double>(-100, -100, 100, 100), 0, 0, 0, isocenter);
            setup_ant.Id = string.Format("Setup_Ant");
            Beam setup_lat = plan.AddSetupBeam(machineParameters, new VRect<double>(-100, -100, 100, 100), 0, 270, 0, isocenter);
            setup_lat.Id = string.Format("Setup_lat");
            Beam cbct = plan.AddSetupBeam(machineParameters, new VRect<double>(-100, -100, 100, 100), 0, 0, 0, isocenter);
            cbct.Id = string.Format("CBCT");

            var myDRR = new DRRCalculationParameters(500); // 500mm is the DRR size
            // Add the layers the args are: (int index, double weight, double ctFrom, double ctTo, double geoFrom, double geoTo) 
            myDRR.SetLayerParameters(0, 1, 100, 1000);
            // Finally apply the myDRR to the beam(s)

            setup_ant.CreateOrReplaceDRR(myDRR);
            setup_lat.CreateOrReplaceDRR(myDRR);
            cbct.CreateOrReplaceDRR(myDRR);
            
            MessageBox.Show("Setup Fields Added Successfully");

        }
  }
}
