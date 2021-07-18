# Kyle Dakin
# 1659221
# NHE2422 Advanced Game Development
# Innovative UI Development of a VR/AR Game

# Code adapted from
# https://www.raywenderlich.com/5475-introduction-to-using-opencv-with-unity - UDP with Unity
# ColorPicker.py - HSV colour picking
# https://aliyasineser.medium.com/calculation-relative-positions-of-aruco-markers-eee9cc4036e3 - Aruco marker tracking
# https://www.learnopencv.com/rotation-matrix-to-euler-angles/ - Rotation matrix to Euler angles

import numpy as np
import cv2
import math
import socket
import cv2.aruco as aruco
import glob

UDP_IP = "127.0.0.1"
UDP_PORT = 52118
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Sets the upper and lower bounds for the Green HSV mask
greenLower = np.array([36, 110, 130])
greenUpper = np.array([84, 255, 255])

# Sets the upper and lower bounds for the Red HSV mask
redLower = np.array([0, 200, 200])
redUpper = np.array([10, 255, 255])

# Sets the criteria for calibrationg the camera to track the Aruco marker
criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)


# This code manages the player controls by watching the phone screen
def calibrate():
    objp = np.zeros((6 * 9, 3), np.float32)
    objp[:, :2] = np.mgrid[0:9, 0:6].T.reshape(-1, 2)
    objpoints = []
    imgpoints = []

    images = glob.glob('calibimages/*.jpg')  # Calibration images, uses a 9*6 chessboard to calibrate the camera
    for fname in images:
        img = cv2.imread(fname)
        grey = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

        ret, corners = cv2.findChessboardCorners(grey, (9, 6), None)
        if ret == True:
            objpoints.append(objp)
            corners2 = cv2.cornerSubPix(grey, corners, (11, 11), (-1, -1), criteria)
            imgpoints.append(corners)
            cv2.drawChessboardCorners(img, (9, 6), corners2, ret)
            cv2.imshow('img', img)
            cv2.waitKey(500)

        ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(objpoints, imgpoints, grey.shape[::-1], None, None)
        return [ret, mtx, dist, rvecs, tvecs]


# Checks if the rotation matrix has been passed is a valid matrix
def isRotationMatrix(R):
    Rt = np.transpose(R)
    identity = np.dot(Rt, R)
    I = np.identity(3, dtype=R.dtype)
    n = np.linalg.norm(I - identity)
    return n < 1e-6


# Converts the rotation matrix into an euler angle as this can be used to determine the direction of the marker
def rotMatrixToEuler(R):
    assert (isRotationMatrix(R))
    sy = math.sqrt(R[0, 0] * R[0, 0] + R[1, 0] * R[1, 0])
    singular = sy < 1e-6
    if not singular:
        x = math.atan2(R[2, 1], R[2, 2])
        y = math.atan2(-R[2, 0], sy)
        z = math.atan2(R[1, 0], R[0, 0])
    else:
        x = math.atan2(-R[1, 2], R[1, 1])
        y = math.atan2(-R[2, 0], sy)
        z = 0
    return np.array([x, y, z])


def track(matrix_coefficients, distortion_coefficients):
    # Open Camera
    try:
        capture = cv2.VideoCapture(1)
    except:
        print("No Camera Source Found!")

    while capture.isOpened():
        instructions = "_"  # String to store all the instructions to be passed to the Unity project to control the player
        # Capture frames from the camera
        success, img = capture.read()

        #First handle the HSV Colour Picking
        imgHSV = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)

        greenMask = cv2.inRange(imgHSV, greenLower, greenUpper)
        redMask = cv2.inRange(imgHSV, redLower, redUpper)

        mask = cv2.bitwise_or(greenMask, redMask)
        result = cv2.bitwise_and(img, img, mask=mask)

        #If there are no Non-zero pixels, that means the colour is present
        if cv2.countNonZero(greenMask):
            instructions += " Move"

        elif cv2.countNonZero(redMask):
            instructions += " Use"

        else:
            instructions += " Stop" #If the screen is not Red or Green (it's white) then stop all action

        cv2.imshow('HSV', result)

        #Aruco marker tracking
        grey = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        aruco_dict = aruco.Dictionary_get(aruco.DICT_4X4_250) #Sets the aruco marker to be tracked
        arucoParameters = aruco.DetectorParameters_create() #Detection parameters
        corners, ids, rejectedImgPoints = aruco.detectMarkers(grey, aruco_dict, parameters=arucoParameters,
                                                              cameraMatrix=matrix_coefficients,
                                                              distCoeff=distortion_coefficients)

        if ids is not None: #If at least one marker has been found
            for i in range(0, len(ids)):
                rvec, tvec, markerpoints = aruco.estimatePoseSingleMarkers(corners[i], 0.1, matrix_coefficients,
                                                                           distortion_coefficients)
                aruco.drawAxis(img, matrix_coefficients, distortion_coefficients, rvec, tvec, 0.1)

                # Rvec is not a valid rotation matrix. CV2.Rodrigues turns Rvec into a valid rotation matrix to
                # calculate Euler angles to determine direction of the marker
                R = cv2.Rodrigues(rvec)
                result = np.rad2deg(rotMatrixToEuler(R[0])[2]) #Gets the Z axis rotation and converts from Radians to Degrees
                if result > -10 and result < 10: # Rotation of the marker can be inconsistent so range helps determine the direction
                    instructions += " Forward"
                elif result > 80 and result < 100:
                    instructions += " Right"
                elif result > 160 and result < 180:
                    instructions += " Back"
                else:
                    instructions += " Left"
        else:
            instructions += " _"

        sock.sendto(instructions.encode(), (UDP_IP, UDP_PORT))
        img = aruco.drawDetectedMarkers(img, corners)
        cv2.imshow('Display', img)

        # Close the camera if 'q' is pressed
        if cv2.waitKey(1) == ord('q'):
            break

    capture.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    ret, mtx, dist, rvecs, tvecs = calibrate()
    track(mtx, dist)
