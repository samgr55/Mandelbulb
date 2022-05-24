using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(int iter, double n, int res, List<Point3d> iPoint, ref object B)
  {
    
    List<Vector3d> vec = new List<Vector3d>();


    for (int i = 0;i < iPoint.Count;i++)
    {
      vec.Add(new Vector3d(iPoint[i].X, iPoint[i].Y, iPoint[i].Z));
    }

    List<double> r = new List<double>();
    List<Point3d> cpt = new List<Point3d>(iPoint);



    for (int i = 0;i < iter;i++)
    {
      foreach (Vector3d v in vec)
        r.Add(v.Length);

      for(int j = 0;j < vec.Count;j++)
      {
        double phi = Math.Atan2(vec[j].Y, vec[j].X);
        double theta = Math.Atan2(Math.Sqrt(vec[j].X * vec[j].X + vec[j].Y * vec[j].Y), vec[j].Z);
        double vr = Math.Pow(r[j], n);
        double vx = Math.Sin(n * theta) * Math.Cos(n * phi);
        double vy = Math.Sin(n * theta) * Math.Sin(n * phi);
        double vz = Math.Cos(n * theta);
        vec[j] = new Vector3d(vx, vy, vz) * vr + (Vector3d) cpt[j];
        cpt[j] += vec[j];
      }

    }

    B = cpt;
  }

  // <Custom additional code> 
  
  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        int iter = default(int);
    if (inputs[0] != null)
    {
      iter = (int)(inputs[0]);
    }

    double n = default(double);
    if (inputs[1] != null)
    {
      n = (double)(inputs[1]);
    }

    int res = default(int);
    if (inputs[2] != null)
    {
      res = (int)(inputs[2]);
    }

    List<Point3d> iPoint = null;
    if (inputs[3] != null)
    {
      iPoint = GH_DirtyCaster.CastToList<Point3d>(inputs[3]);
    }


    //3. Declare output parameters
      object B = null;


    //4. Invoke RunScript
    RunScript(iter, n, res, iPoint, ref B);
      
    try
    {
      //5. Assign output parameters to component...
            if (B != null)
      {
        if (GH_Format.TreatAsCollection(B))
        {
          IEnumerable __enum_B = (IEnumerable)(B);
          DA.SetDataList(1, __enum_B);
        }
        else
        {
          if (B is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(B));
          }
          else
          {
            //assign direct
            DA.SetData(1, B);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}