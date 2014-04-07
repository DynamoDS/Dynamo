##Arduino
###Description
Manages connection to an Arduino microcontroller.

###Inputs
  * **exec** *(object)* - Execution Interval

###Output
  * None


##Kinect
###Description
Read  from a Kinect.

###Inputs
  * **exec** *(object)* - Execution Interval

###Output
  * None


##Leap
###Description
Manages connection to a Leap Motion ViewModel.

###Inputs
  * **Read** *(object)* - Read a frame of data from the Leap

###Output
  * None


##Leap Finger 1
###Description
Reads finger #1 from a Leap Motion hand.



###Output
  * None


##Leap Finger 2
###Description
Reads finger #2 from a Leap Motion hand.



###Output
  * None


##Leap Finger 3
###Description
Reads finger #3 from a Leap Motion hand.



###Output
  * None


##Leap Finger 4
###Description
Reads finger #4 from a Leap Motion hand.



###Output
  * None


##Leap Finger 5
###Description
Reads finger #5 from a Leap Motion hand.



###Output
  * None


##Leap Finger N
###Description
Reads a finger with a specified index from a Leap Motion hand.



###Output
  * None


##Leap Fingers
###Description
Reads the list of fingers from a Leap Motion hand.



###Output
  * None


##Leap Frame
###Description
Current Frame from the Leap Motion ViewModel.

###Inputs
  * **Leap** *(object)* - The Leap ViewModel

###Output
  * None


##Leap Frame N
###Description
Frame from the Leap Motion ViewModel.

###Inputs
  * **Leap** *(object)* - The Leap ViewModel

###Output
  * None


##Leap Frame Rotation
###Description
The angle of rotation around the XYZ axis' derived from the overall rotational motion between the current frame and the specified frame.

###Inputs
  * **Frame** *(object)* - A Frame from a Leap ViewModel

###Output
  * None


##Leap Frame Scale Factor
###Description
The scale factor derived from the overall motion between the current frame and the specified frame.

###Inputs
  * **Frame** *(object)* - A Frame from a Leap ViewModel

###Output
  * None


##Leap Frame Translation
###Description
The change of position derived from the overall linear motion between the current frame and the specified frame.

###Inputs
  * **Frame** *(object)* - A Frame from a Leap ViewModel

###Output
  * None


##Leap Hand
###Description
Reads a hand from a Leap Motion frame.

###Inputs
  * **Frame** *(object)* - The frame of data from the Leap device

###Output
  * None


##Leap Hand 1
###Description
Reads a hand #1 from a Leap Motion frame.

###Inputs
  * **Frame** *(object)* - The frame of data from the Leap device

###Output
  * None


##Leap Hand 2
###Description
Reads a hand #1 from a Leap Motion frame.

###Inputs
  * **Frame** *(object)* - The frame of data from the Leap device

###Output
  * None


##Leap Position
###Description
Reads the position of a Leap Hand, Finger, or Tool.



###Output
  * None


##Leap Tool 1
###Description
Reads tool #1 from a Leap Motion ViewModel.



###Output
  * None


##Leap Tool N
###Description
Reads a tool with a specified index from a Leap Motion ViewModel.



###Output
  * None


##Read Arduino
###Description
Reads values from an Arduino microcontroller.

###Inputs
  * **arduino** *(object)* - Arduino serial connection
  * **range** *(double)* - Number of lines to read

###Output
  * None


##UDP Listener
###Description
Listens for data from the web using a UDP port



###Output
  * None


##Web Request
###Description
Fetches data from the web using a URL.



###Output
  * None


##Write Arduino
###Description
Writes values to an Arduino microcontroller.

###Inputs
  * **arduino** *(object)* - Arduino serial connection
  * **text** *(string)* - Text to be written

###Output
  * None
