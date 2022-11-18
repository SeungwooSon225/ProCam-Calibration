using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;


public class CameraCalibrator : MonoBehaviour
{
    public ScreenShot ScreenShot;

    [SerializeField]
    private GameObject calibrationObject;

    [SerializeField]
    private GameObject plane;
    [SerializeField]
    private Transform[] correspondences;
    [SerializeField]
    public Point[] correspondenceCoordinate = new Point[16];

    private List<Point[]> chessboardCornerCoordinate = new List<Point[]>();

    private Mat cameraIntrinsic = new Mat();
    public Mat homographyMatrix = new Mat();
    Size imageSize = new Size(1920, 1080);


    // Start is called before the first frame update
    void Start()
    {
   
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            //ScreenShot.TakeScreenShot("plane3");
            GetCorrespondenceCoordinate();
            FindChessboardCorner();
            DrawChessboardCorner();
            //ComputeHomography();

            CalibrateCamera();

            //FindHomography();

            calibrationObject.SetActive(false);
        }

    }


    private void GetCorrespondenceCoordinate()
    {
        correspondences = plane.GetComponentsInChildren<Transform>();

        for (int correspondenceIndex = 0; correspondenceIndex < 16; correspondenceIndex++)
        {
            correspondenceCoordinate[correspondenceIndex] = new Point(correspondences[correspondenceIndex + 1].transform.localPosition.x *100,
                                                                      -correspondences[correspondenceIndex + 1].transform.localPosition.y * 100 );
        }
    }


    private void FindChessboardCorner()
    {
        for (int planeIndex = 1; planeIndex < 4; planeIndex++)
        {
            Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/plane" + planeIndex.ToString() + ".png");

            Size patternSize = new Size(4, 4);
            MatOfPoint2f corners = new MatOfPoint2f();

            bool isFind = OpenCVForUnity.Calib3dModule.Calib3d.findChessboardCorners(screenShot, patternSize, corners);

            if (isFind) Debug.Log("find!");
            else Debug.Log("fail!");

            Point[] cornerPoints = corners.toArray();

            chessboardCornerCoordinate.Add(cornerPoints);
        }
    }


    private void DrawChessboardCorner()
    {
        for (int planeIndex = 0; planeIndex < 3; planeIndex++)
        {
            Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/plane" + (planeIndex + 1).ToString() + ".png");

            for (int cornerIndex = 0; cornerIndex < chessboardCornerCoordinate[planeIndex].Length; cornerIndex++)
            {
                Imgproc.circle(screenShot, chessboardCornerCoordinate[planeIndex][cornerIndex], 5, new Scalar(0, 200, 0), 2);
                Imgproc.putText(screenShot, cornerIndex.ToString(), chessboardCornerCoordinate[planeIndex][cornerIndex], Imgproc.FONT_HERSHEY_SIMPLEX, 1, new Scalar(0, 0, 200), 2, Imgproc.LINE_AA, false);
            }

            OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/find corner result" + (planeIndex + 1).ToString() + ".png", screenShot);
        }
    }


    private void CalibrateCamera()
    {
        List<Mat> objectPointsList = new List<Mat>();
        List<Mat> imagePointsList = new List<Mat>();

        List<MatOfPoint3f> objectPointsList3f = new List<MatOfPoint3f>();
        List<MatOfPoint2f> imagePointsList2f = new List<MatOfPoint2f>();

        Mat cameraMatrix = new Mat();
        Mat distCoeffs = new Mat();
        List<Mat> rvecs = new List<Mat>();
        List<Mat> tvecs = new List<Mat>();

        MatOfPoint3f objectPoints = new MatOfPoint3f();
        
        Point3[] point3s = new Point3[16];


        for (int pointIndex = 0; pointIndex < 16; pointIndex++)
        {
            double x = correspondenceCoordinate[pointIndex].x;
            double y = correspondenceCoordinate[pointIndex].y;

            point3s[pointIndex] = new Point3(x, y, 0);
        }

        objectPoints.fromArray(point3s);

        for (int planeIndex = 0; planeIndex < 3; planeIndex++)
        {
            MatOfPoint2f imagePoints = new MatOfPoint2f();

            imagePoints.fromArray(chessboardCornerCoordinate[planeIndex]);
            
            imagePointsList.Add(imagePoints);
            objectPointsList.Add(objectPoints);

            imagePointsList2f.Add(imagePoints);
            objectPointsList3f.Add(objectPoints);
        }

        OpenCVForUnity.Calib3dModule.Calib3d.calibrateCamera(objectPointsList, imagePointsList, imageSize, cameraMatrix, distCoeffs, rvecs, tvecs);

        Debug.Log("------------------------------------------------");
        Debug.Log(cameraMatrix.dump());

        cameraIntrinsic = OpenCVForUnity.Calib3dModule.Calib3d.initCameraMatrix2D(objectPointsList3f, imagePointsList2f, imageSize);

        Debug.Log("------------------------------------------------");
        Debug.Log(cameraIntrinsic.dump());

        Mat rotationMatrix = new Mat();
        OpenCVForUnity.Calib3dModule.Calib3d.Rodrigues(rvecs[0], rotationMatrix);

        Debug.Log("---------ROTATION MATRIX----------------");
        Debug.Log(rotationMatrix.dump());
        Debug.Log(tvecs[0].dump());
        rotationMatrix.put(0, 2, tvecs[0].get(0, 0)[0]);
        rotationMatrix.put(1, 2, tvecs[0].get(1, 0)[0]);
        rotationMatrix.put(2, 2, tvecs[0].get(2, 0)[0]);
        Debug.Log(rvecs[0].dump());
        Debug.Log(rvecs[1].dump());
        Debug.Log(rvecs[2].dump());

        Mat H = cameraMatrix.matMul(rotationMatrix);

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProCamChessboard2.png");
        Mat dstImage = new Mat();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, H, imageSize);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/homography2.png", dstImage);

        Debug.Log("SAVE");

        Mat rvector = new Mat();
        Mat tvector = new Mat();
        MatOfPoint2f imagePoints2f = new MatOfPoint2f();
        imagePoints2f.fromArray(chessboardCornerCoordinate[0]);
        OpenCVForUnity.Calib3dModule.Calib3d.solvePnP(objectPoints, imagePoints2f, cameraIntrinsic, new MatOfDouble(distCoeffs), rvector, tvector);

        OpenCVForUnity.Calib3dModule.Calib3d.Rodrigues(rvector, rotationMatrix);

        Debug.Log("---------CAMERA MATRIX----------------");
        Debug.Log(rotationMatrix.dump());
        Debug.Log(tvector.dump());
        rotationMatrix.put(0, 2, tvector.get(0, 0)[0]);
        rotationMatrix.put(1, 2, tvector.get(1, 0)[0]);
        rotationMatrix.put(2, 2, tvector.get(2, 0)[0]);

        H = cameraIntrinsic.matMul(rotationMatrix);

        homographyMatrix = H.inv();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, homographyMatrix, new Size(3000, 3000));
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/homography3.png", dstImage);

        screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/CheckerBoard2.png");
        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, H, imageSize);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/homography.png", dstImage);
    }


    private void FindCameraIntrinsic()
    {
        MatOfPoint2f objectPoints = new MatOfPoint2f(correspondenceCoordinate);
        MatOfPoint2f imagePoints = new MatOfPoint2f(chessboardCornerCoordinate[2]);
        Mat H1 = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);

        objectPoints = new MatOfPoint2f(correspondenceCoordinate);
        imagePoints = new MatOfPoint2f(chessboardCornerCoordinate[0]);
        Mat H2 = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);

        objectPoints = new MatOfPoint2f(correspondenceCoordinate);
        imagePoints = new MatOfPoint2f(chessboardCornerCoordinate[1]);
        Mat H3 = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);
    }


    private double[] CalculateV(Mat H, int i, int j)
    { 
        double[] v = new double[6];

        v[0] = H.get(0, i)[0] * H.get(0, j)[0];
        v[1] = H.get(0, i)[0] * H.get(1, j)[0] + H.get(1, i)[0] * H.get(0, j)[0];
        v[2] = H.get(2, i)[0] * H.get(0, j)[0] + H.get(0, i)[0] * H.get(2, j)[0];
        v[3] = H.get(1, i)[0] * H.get(1, j)[0];
        v[4] = H.get(2, i)[0] * H.get(1, j)[0] + H.get(1, i)[0] * H.get(2, j)[0];
        v[5] = H.get(2, i)[0] * H.get(2, j)[0];

        return v;
    }


    private void FindHomography()
    {
        MatOfPoint2f objectPoints = new MatOfPoint2f(correspondenceCoordinate);
        MatOfPoint2f imagePoints = new MatOfPoint2f(chessboardCornerCoordinate[0]);

        homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(imagePoints, objectPoints);
        //homographyMatrix = OpenCVForUnity.Calib3dModule.Calib3d.findHomography(objectPoints, imagePoints);

        List<Mat> rotation = new List<Mat>();
        List< Mat > translations = new List<Mat>();
        List<Mat> normals = new List<Mat>();

        OpenCVForUnity.Calib3dModule.Calib3d.decomposeHomographyMat(homographyMatrix, cameraIntrinsic, rotation, translations, normals);

        Debug.Log(translations[0].width());

        Mat screenShot = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread($"{Application.dataPath}/Screenshots/ProCamChessboard.png");
        Mat dstImage = new Mat();

        OpenCVForUnity.ImgprocModule.Imgproc.warpPerspective(screenShot, dstImage, homographyMatrix, new Size(2000,2000));
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite($"{Application.dataPath}/Screenshots/homography.png", dstImage);
        Debug.Log("Save!");
    }
}
