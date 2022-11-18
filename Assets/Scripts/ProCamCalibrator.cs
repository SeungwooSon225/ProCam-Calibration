using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

public class ProCamCalibrator : MonoBehaviour
{
    public ScreenShot ScreenShot;


    private Point[] projectorCorners = new Point[9];
    private Point[] cameraCorners = new Point[9];

    private Mat homographyMatrix = new Mat();
    Size imageSize = new Size(3000, 3000);

    // Start is called before the first frame update
    void Start()
    {
        FindChessboardCorner();
        FindHomography();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            ScreenShot.TakeScreenShot("ProCamChessboard2");
        }
    }


    private void FindChessboardCorner()
    {
        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProCamImage.png", 1);

        Size patternSize = new Size(9, 9);
        MatOfPoint2f corners = new MatOfPoint2f();

        bool isFind = OpenCVForUnity.Calib3dModule.Calib3d.findChessboardCorners(screenShot, patternSize, corners);

        if (isFind) Debug.Log("find!");
        else Debug.Log("fail!");

        Point[] cornerPoints = corners.toArray();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                cameraCorners[i * 3 + j] = cornerPoints[i * 36 + j * 4];
            }
        }

        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/CheckerBoard.png", 1);

        isFind = OpenCVForUnity.Calib3dModule.Calib3d.findChessboardCorners(screenShot, patternSize, corners);

        if (isFind) Debug.Log("find!");
        else Debug.Log("fail!");

        cornerPoints = corners.toArray();

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                projectorCorners[i * 3 + j] = cornerPoints[i * 36 + j * 4];
            }
        }
    }

    private void FindHomography()
    {
        MatOfPoint2f objectPoints = new MatOfPoint2f(projectorCorners);
        MatOfPoint2f imagePoints = new MatOfPoint2f(cameraCorners);

        homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);
        //homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(objectPoints, imagePoints);

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProCamChessboard2.png");
        Mat dstImage = new Mat();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, homographyMatrix * 100, imageSize);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/ProCam Homography.png", dstImage);
        Debug.Log("Save!");
    }
}
