using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ABB.Robotics.Math;
using ABB.Robotics.RobotStudio;
using ABB.Robotics.RobotStudio.Stations;

namespace RSCompareCloudsOfPoints
{
	/// <summary>
	/// Code-behind class for the RSCompareCloudsOfPoints Smart Component.
	/// </summary>
	/// <remarks>
	/// The code-behind class should be seen as a service provider used by the 
	/// Smart Component runtime. Only one instance of the code-behind class
	/// is created, regardless of how many instances there are of the associated
	/// Smart Component.
	/// Therefore, the code-behind class should not store any state information.
	/// Instead, use the SmartComponent.StateCache collection.
	/// </remarks>
	public class CodeBehind : SmartComponentCodeBehind
	{
		/// <summary>
		///   Called when the value of a dynamic property value has changed.
		/// </summary>
		/// <param name="component"> Component that owns the changed property. </param>
		/// <param name="changedProperty"> Changed property. </param>
		/// <param name="oldValue"> Previous value of the changed property. </param>
		public override void OnPropertyValueChanged(SmartComponent component, DynamicProperty changedProperty, Object oldValue)
		{
			PointCloud p = null;
			Station station = Project.ActiveProject as Station;
			if (station == null) return;

			//Get last cloud
			for (int i = 0; i < station.PointClouds.Count; ++i)
				if (station.PointClouds[i].Name == component.UniqueId)
					p = station.PointClouds[i];

			if (p == null) return;

			if (changedProperty.Name == "Transform")
				p.Transform.GlobalMatrix = (Matrix4)changedProperty.Value;

			if (changedProperty.Name == "Visible")
				p.Visible = (bool)changedProperty.Value;
		}

		/// <summary>
		///   Called when the value of an I/O signal value has changed.
		/// </summary>
		/// <param name="component"> Component that owns the changed signal. </param>
		/// <param name="changedSignal"> Changed signal. </param>
		public override void OnIOSignalValueChanged(SmartComponent component, IOSignal changedSignal)
		{
			if (changedSignal.Name == "Open" && (int)changedSignal.Value == 1)
			{
				Single min = Single.MaxValue, max = Single.MinValue;
				Dictionary<int, Dictionary<int, Single>> points_1 = OpenFile(component, "FileName_1", ref min, ref max);
				if (points_1 != null)
				{
					Dictionary<int, Dictionary<int, Single>> points_2 = OpenFile(component, "FileName_2", ref min, ref max);
					if (points_2 != null)
					{

						Single amplitude = max - min;

						//Now create CloudOfPoint
						PointCloud p = new PointCloud
						{
							PointSize = 2,                // size of the points
							Visible = (bool)component.Properties["Visible"].Value, // visibility of points
						};
						p.Transform.Matrix = (Matrix4)component.Properties["Transform"].Value; // transformation of points

						Double epsilon = (Double)component.Properties["Epsilon"].Value;
						//Use List as we cannot know how many point differs now.
						List<Vector3> points = new List<Vector3>();
						List<Color> colors = new List<Color>();

						//Search for all X values of File1 if get something in File2
						foreach (KeyValuePair<int, Dictionary<int, Single>> listY_1 in points_1)
						{
							if (points_2.TryGetValue(listY_1.Key, out Dictionary<int, Single> listY_2))
							{
								//If X_1 is on File2, scan all Y_1
								foreach (KeyValuePair<int, Single> z_1 in listY_1.Value)
								{
									if (listY_2.TryGetValue(z_1.Key, out Single z_2))
									{
										//Get same point, compare it and create it in middle, color it depends delta
										Single delta = z_2 - z_1.Value;
										int color = (int)((Math.Abs(delta) / amplitude) * 255);
										points.Add(new Vector3(listY_1.Key * epsilon, z_1.Key * epsilon, z_1.Value + delta / 2)); // place the point
										colors.Add(Color.FromArgb(255, color, 255 - color, 0));
									}
									else
									{
										//Point is not in File2, add it in gray
										points.Add(new Vector3(listY_1.Key * epsilon, z_1.Key * epsilon, z_1.Value)); // place the point
										colors.Add(Color.FromArgb(255, 127, 127, 127));
									}
								}

							}
							else
							{
								//If X_1 is not on File2, add all of them in gray
								foreach (KeyValuePair<int, Single> z in listY_1.Value)
								{
									points.Add(new Vector3(listY_1.Key * epsilon, z.Key * epsilon, z.Value)); // place the point
									colors.Add(Color.FromArgb(255, 127, 127, 127));
								}
							}
						}

						//Now Search for all X values of File2 if get something in File1
						foreach (KeyValuePair<int, Dictionary<int, Single>> listY_2 in points_2)
						{
							if (points_1.TryGetValue(listY_2.Key, out Dictionary<int, Single> listY_1))
							{
								//If X_2 is on File1, scan all Y_2
								foreach (KeyValuePair<int, Single> z_2 in listY_2.Value)
								{
									if (listY_1.TryGetValue(z_2.Key, out Single z_1))
									{
										//Get same point, already done before
									}
									else
									{
										//Point is not in File1, add it in gray blue
										points.Add(new Vector3(listY_2.Key * epsilon, z_2.Key * epsilon, z_2.Value)); // place the point
										colors.Add(Color.FromArgb(255, 127, 127, 255));
									}
								}

							}
							else
							{
								//If X_1 is not on File2, add all of them in gray blue
								foreach (KeyValuePair<int, Single> z in listY_2.Value)
								{
									points.Add(new Vector3(listY_2.Key * epsilon, z.Key * epsilon, z.Value)); // place the point
									colors.Add(Color.FromArgb(255, 127, 127, 255));
								}
							}
						}

						p.Points = points.ToArray();
						p.Colors = colors.ToArray();
						p.Name = component.UniqueId;

						Station station = Project.ActiveProject as Station;
						//Remove last cloud
						for (int i = 0; i < station.PointClouds.Count; ++i)
							if (station.PointClouds[i].Name == component.UniqueId)
								station.PointClouds.RemoveAt(i);

						station.PointClouds.Add(p); // Add the pointCloud
					}
				}
				component.IOSignals["Open"].Value = false;
			}
			if (changedSignal.Name == "Delete" && (int)changedSignal.Value == 1)
			{
				Station station = Project.ActiveProject as Station;
				//Remove last cloud
				for (int i = 0; i < station.PointClouds.Count; ++i)
					if (station.PointClouds[i].Name == component.UniqueId)
						station.PointClouds.RemoveAt(i);

				component.IOSignals["Delete"].Value = false;
			}
		}

		/// <summary>
		///   Open a CloudOfPoint file to be compare
		/// </summary>
		/// <param name="component">Smart Component</param>
		/// <param name="propertyName">Smart Component property with filename text</param>
		/// <param name="min">Return minimal Z value</param>
		/// <param name="max">Return maximal Z value</param>
		/// <returns>A Dictionary (with X as Key and a Dictionary (with y as key and z as value) as value) for all points</returns>
		private Dictionary<int, Dictionary<int, Single>> OpenFile(SmartComponent component, string propertyName, ref Single min, ref Single max)
		{
			Stream myStream = null;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			String fileName = component.Properties[propertyName].Value.ToString();
			if (fileName != "")
			{
				try
				{
					FileInfo fileInfo = new FileInfo(fileName);
					if (fileInfo.Exists)
						openFileDialog.InitialDirectory = fileInfo.DirectoryName;
				}
				catch (Exception)
				{
					fileName = fileName + " Bad Name";
				}

				openFileDialog.FileName = fileName;
			}
			openFileDialog.Filter = "Cloud of Points (*.bin)|*.bin|All files (*.*)|*.*";
			openFileDialog.FilterIndex = 1;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.Title = "Open " + propertyName;

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if ((myStream = openFileDialog.OpenFile()) != null)
					{
						using (myStream)
						{
							//Get 4 First bytes to get file size
							byte[] b = new byte[4];
							myStream.Read(b, 0, 4);
							int len = BitConverter.ToInt32(b, 0);
							if ((len * 3 * 4) != (myStream.Length - 4))
							{
								MessageBox.Show("Error: Bad formated file. Indicates to have size of: " + len + " bytes but get: " + (myStream.Length - 4));
								return null;
							}

							//Now create CloudOfPoint
							Dictionary<int, Dictionary<int,Single>> points = new Dictionary<int, Dictionary<int, Single>>();

							Double unit = (Double)component.Properties["Unit"].Value;
							Double epsilon = (Double)component.Properties["Epsilon"].Value;
							bool averaging = (bool)component.Properties["Averaging"].Value;

							// Set the position and color of every single point
							for (uint curs = 0; curs < len; ++curs)
							{
								myStream.Read(b, 0, 4);
								int x = (int)(BitConverter.ToSingle(b, 0) * unit / epsilon);
								myStream.Read(b, 0, 4);
								int y = (int)(BitConverter.ToSingle(b, 0) * unit / epsilon);
								myStream.Read(b, 0, 4);
								Single z = (Single)(BitConverter.ToSingle(b, 0) * unit);
								min = Math.Min(min, z);
								max = Math.Max(max, z);
								Dictionary<int, Single> listY;
								if (!points.TryGetValue(x, out listY))
								{
									listY = new Dictionary<int, Single>();
									points.Add(x, listY);
									listY.Add(y, z);
								} else
								{
									if (!listY.TryGetValue(y, out Single oldz))
										listY.Add(y, z);
									else
										if (averaging)
											listY[y] = oldz + (z-oldz) / 2;
								}
							}

							component.Properties[propertyName].Value = openFileDialog.FileName;
							return points;
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
				}
			}
			return null;
		}

	}
}
