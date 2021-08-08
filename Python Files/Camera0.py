# Kyle Dakin
# NHE2422 Advanced Game Development
# Innovative UI Development of a VR/AR Game

# This code controls the camera movement by tracking the players head movement
# Code adapted from:
# https://www.raywenderlich.com/5475-introduction-to-using-opencv-with-unity
# FaceDetection practical

import cv2
import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 52121
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

face_cascade = cv2.CascadeClassifier('data/haarcascades/haarcascade_frontalface_default.xml')

try:
    capture = cv2.VideoCapture(0)
except:
    print("No Camera Source Found!")

while capture.isOpened():
    #Gets the width and height of the frame is it uses this to work out which direction you are going in
    #Makes it scale with different frames
    frameHeight = int(capture.get(4))
    frameWidth = int(capture.get(3))

    #String to be passed to Unity as the method of communicated instructions to the game
    directions = "_"

    _, img = capture.read()
    grey = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    faces = face_cascade.detectMultiScale(grey, 1.1, 4)
    for (x, y, w, h) in faces:
        cv2.rectangle(img, (x, y), (x + w, y + h), (255, 0, 0), 2)

    if x > frameWidth/2:
        directions += " Left"
    elif x < frameWidth/10:
        directions += " Right"
    else:
        directions += " _"

    if y < frameHeight/10:
        directions += " Up"
    elif y > frameHeight/2:
        directions += " Down"
    else:
        directions += " _"

    #Calculates the area of the square drawn on screen and compares it to the area of the frame
    #Size of the square in comparison to the frame controls zom in/out controls
    if w*h > frameHeight*frameWidth/2:
        directions += " Forward"
    elif w*h < frameHeight*frameWidth/10:
        directions += " Backwards"
    else:
        directions += " _"

    #Sends the encoded directions to the Unity project
    sock.sendto(directions.encode(), (UDP_IP, UDP_PORT))
    #Displays the facial tracking on screen
    cv2.imshow('img', img)

    # Close the camera if 'q' is pressed
    if cv2.waitKey(1) == ord('q'):
        break

capture.release()
cv2.destroyAllWindows()
