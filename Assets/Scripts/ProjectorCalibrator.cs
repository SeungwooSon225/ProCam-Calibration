using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

public class ProjectorCalibrator : MonoBehaviour
{
    public ScreenShot ScreenShot;
    public CameraCalibrator CameraCalibrator;

    private List<Point[]> chessboardCornerCoordinate = new List<Point[]>();

    [SerializeField]
    private GameObject[] planes;
    [SerializeField]
    private List<Point3[]> correspondenceCoordinate3f = new List<Point3[]>();
    private List<Point[]> correspondenceCoordinate2f = new List<Point[]>();

    private Mat homographyMatrix = new Mat();
    private Mat cameraIntrinsic = new Mat();
    Size imageSize = new Size(1920, 1080);


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            //ScreenShot.TakeScreenShot("ProjectorImage");
            FindChessboardCorner();
            DrawChessboardCorner();
            GetCorrespondenceCoordinate();
            FindHomography();
            CalibrateProjector();
        }
    }


    private void GetCorrespondenceCoordinate()
    {
        for (int planeIndex = 0; planeIndex < 3; planeIndex++)
        {
            Transform[] correspondences = planes[planeIndex].GetComponentsInChildren<Transform>(); ;

            Point3[] correspondence3f = new Point3[9];
            Point[] correspondence2f = new Point[9];

            for (int correspondenceIndex = 0; correspondenceIndex < 9; correspondenceIndex++)
            {
                correspondence3f[correspondenceIndex] = new Point3(correspondences[correspondenceIndex + 1].transform.localPosition.x * 100,
                                                                          -correspondences[correspondenceIndex + 1].transform.localPosition.y * 100, 0);
                correspondence2f[correspondenceIndex] = new Point(correspondences[correspondenceIndex + 1].transform.localPosition.x * 100,
                                                                          -correspondences[correspondenceIndex + 1].transform.localPosition.y * 100);
            }

            correspondenceCoordinate3f.Add(correspondence3f);
            correspondenceCoordinate2f.Add(correspondence2f);
        }
    }


    private void FindChessboardCorner()
    {
        //for (int planeIndex = 1; planeIndex < 4; planeIndex++)
        //{
        //    Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProjectorImage" + (planeIndex).ToString() +  ".png", 1);

        //    Size patternSize = new Size(9, 9);
        //    MatOfPoint2f corners = new MatOfPoint2f();

        //    bool isFind = OpenCVForUnity.Calib3dModule.Calib3d.findChessboardCorners(screenShot, patternSize, corners);

        //    if (isFind) Debug.Log("find!");
        //    else Debug.Log("fail!");

        //    Point[] cornerPoints = corners.toArray();

        //    chessboardCornerCoordinate.Add(cornerPoints);
        //}

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProjectorImage.png", 1);

        Size patternSize = new Size(9, 9);
        MatOfPoint2f corners = new MatOfPoint2f();

        bool isFind = OpenCVForUnity.Calib3dModule.Calib3d.findChessboardCorners(screenShot, patternSize, corners);

        if (isFind) Debug.Log("find!");
        else Debug.Log("fail!");

        Point[] cornerPoints = corners.toArray();

        Point[] cornerPoints9 = new Point[9];

        for (int i = 0; i < 3; i++)
        { 
            for (int j = 0; j < 3; j++)
            {
                cornerPoints9[i * 3 + j] = cornerPoints[i * 36 + j * 4];
            }
        }

        chessboardCornerCoordinate.Add(cornerPoints9);
    }


    private void DrawChessboardCorner()
    {
        //for (int planeIndex = 0; planeIndex < 3; planeIndex++)
        //{
        //    Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProjectorImage" + (planeIndex + 1).ToString() + ".png");

        //    for (int cornerIndex = 0; cornerIndex < chessboardCornerCoordinate[planeIndex].Length; cornerIndex++)
        //    {
        //        Imgproc.circle(screenShot, chessboardCornerCoordinate[planeIndex][cornerIndex], 5, new Scalar(0, 200, 0), 2);
        //        Imgproc.putText(screenShot, cornerIndex.ToString(), chessboardCornerCoordinate[planeIndex][cornerIndex], Imgproc.FONT_HERSHEY_SIMPLEX, 1, new Scalar(0, 0, 200), 2, Imgproc.LINE_AA, false);
        //    }

        //    OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/ProjectorImage corner result" + (planeIndex + 1).ToString() + ".png", screenShot);
        //}

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProjectorImage.png", 1);

        for (int cornerIndex = 0; cornerIndex < chessboardCornerCoordinate[0].Length; cornerIndex++)
        {
            Imgproc.circle(screenShot, chessboardCornerCoordinate[0][cornerIndex], 5, new Scalar(0, 200, 0), 2);
            Imgproc.putText(screenShot, cornerIndex.ToString(), chessboardCornerCoordinate[0][cornerIndex], Imgproc.FONT_HERSHEY_SIMPLEX, 1, new Scalar(0, 0, 200), 2, Imgproc.LINE_AA, false);
        }

        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/ProjectorImage corner result.png", screenShot);
    }


    private void FindHomography()
    {
        MatOfPoint2f objectPoints = new MatOfPoint2f(correspondenceCoordinate2f[0]);
        MatOfPoint2f imagePoints = new MatOfPoint2f(chessboardCornerCoordinate[0]);

        homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);
        homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(objectPoints, imagePoints);

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/homography3.png");
        Mat dstImage = new Mat();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, homographyMatrix, new Size(2000, 2000));
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/projector homography3.png", dstImage);
        Debug.Log("Save!");
    }

    private void CalibrateProjector()
    {
        List<Mat> objectPointsList = new List<Mat>();
        List<Mat> imagePointsList = new List<Mat>();

        List<MatOfPoint3f> objectPointsList3f = new List<MatOfPoint3f>();
        List<MatOfPoint2f> imagePointsList2f = new List<MatOfPoint2f>();

        Mat cameraMatrix = new Mat();
        Mat distCoeffs = new Mat();
        List<Mat> rvecs = new List<Mat>();
        List<Mat> tvecs = new List<Mat>();

        
        for (int planeIndex = 0; planeIndex < 3; planeIndex++)
        {
            MatOfPoint2f imagePoints = new MatOfPoint2f();
            MatOfPoint3f objectPoints = new MatOfPoint3f();

            imagePoints.fromArray(chessboardCornerCoordinate[0]);
            objectPoints.fromArray(correspondenceCoordinate3f[planeIndex]);

            imagePointsList.Add(imagePoints);
            objectPointsList.Add(objectPoints);

            imagePointsList2f.Add(imagePoints);
            objectPointsList3f.Add(objectPoints);
        }

        OpenCVForUnity.Calib3dModule.Calib3d.calibrateCamera(objectPointsList, imagePointsList, imageSize, cameraMatrix, distCoeffs, rvecs, tvecs);
        cameraIntrinsic = OpenCVForUnity.Calib3dModule.Calib3d.initCameraMatrix2D(objectPointsList3f, imagePointsList2f, imageSize);

        Mat rotationMatrix = new Mat();
        OpenCVForUnity.Calib3dModule.Calib3d.Rodrigues(rvecs[1], rotationMatrix);

        Debug.Log("---------ROTATION MATRIX----------------");
        Debug.Log(rotationMatrix.dump());
        rotationMatrix.put(0, 2, tvecs[1].get(0, 0)[0]);
        rotationMatrix.put(1, 2, tvecs[1].get(1, 0)[0]);
        rotationMatrix.put(2, 2, tvecs[1].get(2, 0)[0]);
        Debug.Log(rvecs[0].dump());
        Debug.Log(rvecs[1].dump());
        Debug.Log(rvecs[2].dump());

        Mat H = cameraMatrix.matMul(rotationMatrix);

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/projector homography.png");
        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/homography3.png");
        Mat dstImage = new Mat();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, H, new Size(1020,1020));
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/projector homography2.png", dstImage);

        Debug.Log("SAVE");


        Mat rvector = new Mat();
        Mat tvector = new Mat();
        MatOfPoint2f imagePoints2f = new MatOfPoint2f();
        imagePoints2f.fromArray(chessboardCornerCoordinate[0]);
        MatOfPoint3f objectPoints3f = new MatOfPoint3f();
        objectPoints3f.fromArray(correspondenceCoordinate3f[0]);
        OpenCVForUnity.Calib3dModule.Calib3d.solvePnP(objectPoints3f, imagePoints2f, cameraIntrinsic, new MatOfDouble(distCoeffs), rvector, tvector);

        OpenCVForUnity.Calib3dModule.Calib3d.Rodrigues(rvector, rotationMatrix);

        Debug.Log("---------ROTATION MATRIX----------------");
        Debug.Log(rotationMatrix.dump());
        rotationMatrix.put(0, 2, tvector.get(0, 0)[0]);
        rotationMatrix.put(1, 2, tvector.get(1, 0)[0]);
        rotationMatrix.put(2, 2, tvector.get(2, 0)[0]);

        H = cameraIntrinsic.matMul(rotationMatrix);
        homographyMatrix = H.matMul(CameraCalibrator.homographyMatrix);
        homographyMatrix = CameraCalibrator.homographyMatrix.matMul(H);

        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProCamChessboard2.png");
        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, homographyMatrix, new Size(10000,6000));
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/ProCam Homography result.png", dstImage);


        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/homography src.png");
        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, H, imageSize);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/projector homography4.png", dstImage);


        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/projector homography.png");
        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, H, imageSize);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/projector homography src.png", dstImage);
    }
}
