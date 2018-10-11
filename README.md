# RSCompareCloudsOfPoints
This is a RobotStudio Smart Component to Compare two Clouds of Points in .bin format.
Colored from green for same Z to Red for maximal amplitude.
## What you have to do before compiling:
  - Update ABB.Robotics.* References to Good RobotStudio SDK Version path with ***Project*** - ***Add Reference*** - ***Browse***.
  - On Project Properties:
    - **Application**: Choose good .NET Framework version.
    - **Build Events**: *Post Build Events*: Replace with the good LibraryCompiler.exe Path.
    - **Debug**: *Start External Program*: Replace with the good RobotStudio.exe Path.
  - In *\RSCompareCloudsOfPoints\RSCompareCloudsOfPoints.en.xml*:
    - Replace **xsi:schemaLocation** value with good one.
  - Same for *\RSCompareCloudsOfPoints\RSCompareCloudsOfPoints.xml*.

### If your project path is on network drive:
##### To get RobotStudio load it:
  - In *$(RobotStudioPath)\Bin\RobotStudio.exe.config* file:
    - Add in section *`<configuration><runtime>`*
      - `<loadFromRemoteSources enable="true"/>`

##### To Debug it:
  - Start first RobotStudio to get RobotStudio.exe.config loaded.
  - Then attach its process in VisualStudio ***Debug*** - ***Attach to Process..***
  
## Usage
![RSCompareCloudsOfPoints](https://raw.githubusercontent.com/DenisFR/RSCompareCloudsOfPoints/master/RSCompareCloudsOfPoints/RSCompareCloudsOfPoints.jpg)
### Properties
  - ***FileName_1***:\
FileName of First Cloud of Points. Colored in gray if not found in other file. (Read Only)
  - ***FileName_2***:\
FileName of Second Cloud of Points. Colored in gray-blue if not found in other file. (Read Only)
  - ***Unit***:\
Unit/Scale of Cloud of Points. (For the next Open)
  - ***Epsilon***:\
Roundoff for X and Y values of Cloud of Points to compare. (For the next Open)
  - ***Averaging***:\
Averaging Z values for same X and Y point. Else only get first value. (For the next Open)
  - ***Transform***:\
Transformation of Result Cloud of Points.
  - ***Visible***:\
If Result Cloud of Points is visible.
### Signals
  - ***Open***:\
Open two Clouds of Points file to Compare. (Clear Olders)
  - ***Delete***:\
Delete Clouds of Points. (Use it before removing Component)
