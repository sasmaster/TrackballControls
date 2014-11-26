/**
* Initial @author Eberhard Graether / http://egraether.com/
* C# port by Michael Ivanov (sasmaster) 
*
* This utility was written for OpenTK SDK.
* It wasn't tested with other C# based graphics libraries.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using System.Diagnostics;
namespace OpenGL_Tess
{
    enum State
    {
        NONE,
        ROTATE,
        ZOOM,
        PAN
    };
    public class Camera3D
    {

        private Vector3 _up;

        public Vector3 Up
        {
            get { return _up; }
            set { _up = value; }
        }
        public Vector3 _pos;
        public Vector3 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        private Matrix4 _viewMatrix;

        public Matrix4 ViewMatrix
        {
            get { return _viewMatrix; }
            set { _viewMatrix = value; }
        }

        public Camera3D()
        {
            _pos = Vector3.Zero;
            _up = new Vector3(0.0f, 1.0f, 0.0f);
            _viewMatrix = Matrix4.Identity;
        }
        public void LookAt(Vector3 target)
        {

            _viewMatrix = Matrix4.LookAt(_pos, target, _up);

        }
    }

    public class TrackBallControlls
    {
	    private const double SQRT1_2 = 0.7071067811865476;
        public Camera3D _camObject;
        Rectangle _screen;
        State _state, _prevState;
        private bool _enabled;
        private bool _staticMoving;

        private float _rotateSpeed;

        public float RotateSpeed
        {
            get { return _rotateSpeed; }
            set { _rotateSpeed = value; }
        }
        private float _zoomSpeed;

        public float ZoomSpeed
        {
            get { return _zoomSpeed; }
            set { _zoomSpeed = value; }
        }
        private float _panSpeed;

        public float PanSpeed
        {
            get { return _panSpeed; }
            set { _panSpeed = value; }
        }
        private bool _noRotate;

        public bool NoRotate
        {
            get { return _noRotate; }
            set { _noRotate = value; }
        }
        private bool _noZoom;

        public bool NoZoom
        {
            get { return _noZoom; }
            set { _noZoom = value; }
        }
        private bool _noPan;

        public bool NoPan
        {
            get { return _noPan; }
            set { _noPan = value; }
        }
        private bool _noRoll;

        public bool NoRoll
        {
            get { return _noRoll; }
            set { _noRoll = value; }
        }


        public bool StaticMoving
        {
            get { return _staticMoving; }
            set { _staticMoving = value; }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        private float _dynamicDampingFactor;

        public float DynamicDampingFactor
        {
            get { return _dynamicDampingFactor; }
            set { _dynamicDampingFactor = value; }
        }

        private float _minDistance;

        public float MinDistance
        {
            get { return _minDistance; }
            set { _minDistance = value; }
        }
        private float _maxDistance;

        public float MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }
        Vector3 _target;
        Vector3 _eye;
        Vector3 _rotateStart, _rotateEnd;
        Vector2 _zoomStart, _zoomEnd;
        Vector2 _panStart, _panEnd;
        Vector3 lastPosition;
        List<int> _keys;

        public TrackBallControlls(Camera3D camObject, Rectangle screenSize, GameWindow win)
        {
            _camObject = camObject;
            _enabled = true;


            _screen = screenSize;

            _rotateSpeed = 1.0f;
            _zoomSpeed = 1.2f;
            _panSpeed = 0.3f;

            _noRotate = false;
            _noZoom = false;
            _noPan = false;
            _noRoll = false;

            _staticMoving = false;
            _dynamicDampingFactor = 0.2f;

            _minDistance = 0.0f;
            _maxDistance = float.PositiveInfinity; 

            _target = Vector3.Zero;

            lastPosition = Vector3.Zero;

            _state = State.NONE;
            _prevState = State.NONE;

            _eye = Vector3.Zero;

            _rotateStart = Vector3.Zero;
            _rotateEnd = Vector3.Zero;

            _zoomStart = Vector2.Zero;
            _zoomEnd = Vector2.Zero;

            _panStart = Vector2.Zero;
            _panEnd = Vector2.Zero;

            _keys = new List<int> { 65 /*A*/, 83 /*S*/, 68 /*D*/};

            win.MouseMove += OpenGLTess_MouseMove;
            win.MouseUp += OpenGLTess_MouseUp;
            win.MouseDown += OpenGLTess_MouseDown;
            win.MouseWheel += OpenGLTess_MouseWheel;
            win.KeyUp += OpenGLTess_KeyUp;
            win.KeyDown += OpenGLTess_KeyDown;



        }

        Vector2 GetMouseOnScreen(int clientX, int clientY)
        {
            return new Vector2(
                (float)(clientX - _screen.Left) / (float)_screen.Width,
                (float)(clientY - _screen.Top) / (float)_screen.Height
                );
        }
        
        Vector3 GetMouseProjectionOnBall(int clientX, int clientY)
        {
            Vector3 mouseOnBall = new Vector3(
              ((float)clientX - (float)_screen.Width * 0.5f) / (float)(_screen.Width * 0.5f),
               ((float)_screen.Height * 0.5f - (float)clientY) / (float)(_screen.Height * 0.5f),
               0.0f
            );

            double length = mouseOnBall.Length;

            if (_noRoll)
            {

                if (length < SQRT1_2)
                {

                    mouseOnBall.Z = (float)Math.Sqrt(1.0 - length * length);

                }
                else
                {

                    mouseOnBall.Z = (float)(0.5 / length);

                }

            }
            else if (length > 1.0)
            {

                mouseOnBall.Normalize();

            }
            else
            {

                mouseOnBall.Z = (float)Math.Sqrt(1.0 - length * length);

            }

            Vector3 camPos = _camObject.Pos;

            _eye = Vector3.Subtract(camPos, _target);


            Vector3 upClone = new Vector3(_camObject.Up);
            Vector3 projection;//object.up.clone().normalize().scale(mouseOnBall.y);
            upClone.Normalize();


            projection = Vector3.Multiply(upClone, mouseOnBall.Y);


            //  projection.add(object.up.cross(_eye).normalize().scale(mouseOnBall.x));
            Vector3 cross = Vector3.Cross(_camObject.Up, _eye);
            cross.Normalize();
            cross = Vector3.Multiply(cross, mouseOnBall.X);
            projection = Vector3.Add(projection, cross);

            //  projection.add(_eye.normalize().scale(mouseOnBall.z));
            Vector3 eyeClone = new Vector3(_eye);
            eyeClone.Normalize();
            projection = Vector3.Add(projection, Vector3.Multiply(eyeClone, mouseOnBall.Z));

            return projection;

        }

        void RotateCamera()
        {


            float angle = (float)Math.Acos(Vector3.Dot(_rotateStart, _rotateEnd) / _rotateStart.Length / _rotateEnd.Length);

            if (!float.IsNaN(angle) && angle != 0.0f)
            {


                Vector3 axis = Vector3.Cross(_rotateStart, _rotateEnd); //_rotateStart.cross(_rotateEnd).normalize();
                axis.Normalize();
                if (float.IsNaN(axis.X))
                {
                    axis = Vector3.Zero; /// a hack,sometimes NAN comes from "axis" and fucks up everything. Zeroing of it resolves the issue.

                }

                Quaternion quaternion = Quaternion.Identity;

                angle *= _rotateSpeed;

                //  quaternion.setAxisAngle(axis, angle);
                quaternion = Quaternion.FromAxisAngle(axis, -angle);

                //quaternion.rotate(_eye);
                _eye = Vector3.Transform(_eye, quaternion);            

                //  quaternion.rotate(object.up);
                _camObject.Up = Vector3.Transform(_camObject.Up, quaternion);
                //  quaternion.rotate(_rotateEnd);
                _rotateEnd = Vector3.Transform(_rotateEnd, quaternion);


                if (_staticMoving)
                {

                    _rotateStart = new Vector3(_rotateEnd);

                }
                else
                {

                    quaternion = Quaternion.FromAxisAngle(axis, angle * (_dynamicDampingFactor - 1.0f));


                    _rotateStart = Vector3.Transform(_rotateStart, quaternion);

                }



            }

        }
        void ZoomCamera()
        {

            float factor = 1.0f + (float)(_zoomEnd.Y - _zoomStart.Y) * _zoomSpeed;

            if (factor != 1.0f && factor > 0.0f)
            {

                //  _eye.scale( factor );
                _eye = Vector3.Multiply(_eye, (float)factor);

                if (_staticMoving)
                {

                    _zoomStart = new Vector2(_zoomEnd.X, _zoomEnd.Y);

                }
                else
                {

                    _zoomStart.Y += (float)(_zoomEnd.Y - _zoomStart.Y) * _dynamicDampingFactor;

                }

            }



        }

        void PanCamera()
        {

            Vector2 mouseChange = _panEnd - _panStart;

            if (mouseChange.Length != 0.0f)
            {

                // mouseChange.scale( _eye.Length * _panSpeed );
                mouseChange = Vector2.Multiply(mouseChange, _eye.Length * _panSpeed);

                //   Vector3 pan = _eye.cross( object.up ).normalize().scale( mouseChange.x );
                Vector3 pan = Vector3.Cross(_eye, _camObject.Up);
                pan.Normalize();
                pan = Vector3.Multiply(pan, mouseChange.X);

                // pan += object.up.clone().normalize().scale( mouseChange.Y );
                Vector3 camUpClone = new Vector3(_camObject.Up);
                camUpClone.Normalize();
                camUpClone = Vector3.Multiply(camUpClone, mouseChange.Y);
                pan += camUpClone;

                //object.position.add( pan );
                _camObject._pos = Vector3.Add(_camObject.Pos, pan);
              

                //  target.add( pan );
                _target = Vector3.Add(_target, pan);
                if (_staticMoving)
                {

                    _panStart = _panEnd;

                }
                else
                {
                    Vector2 diff = _panEnd - _panStart;
                    diff = Vector2.Multiply(diff, _dynamicDampingFactor);
                    _panStart += diff;// (_panEnd - _panStart).scale(_dynamicDampingFactor);

                }

            }



        }
        void CheckDistances()
        {

            if (!_noZoom || !_noPan)
            {



                if (_camObject.Pos.LengthSquared > _maxDistance * _maxDistance)
                {


                    _camObject._pos.Normalize();

                    _camObject._pos = Vector3.Multiply(_camObject._pos, _maxDistance);


                }

                if (_eye.LengthSquared < _minDistance * _minDistance)
                {

                    // object.position = target + _eye.normalize().scale(minDistance);
                    _eye.Normalize();
                    _eye = Vector3.Multiply(_eye, _minDistance);

                    _camObject._pos = _target + _eye;


                }

            }

        }


        public void Update()
        {

            //   _eye.setFrom( object.position ).sub( target );
            _eye = new Vector3(_camObject.Pos);
         
            _eye = Vector3.Subtract(_eye, _target);
            if (!_noRotate)
            {
                RotateCamera();
            }

            if (!_noZoom)
            {
                ZoomCamera();
            }

            if (!_noPan)
            {
                PanCamera();
            }

            // object.position =  target + _eye;
            _camObject._pos = _target + _eye;

            CheckDistances();

            // object.lookAt( target );
            _camObject.LookAt(_target);



            // distanceToSquared
            if ((lastPosition - _camObject.Pos).LengthSquared > 0.0f)
            {
                //
                //   dispatchEvent( changeEvent );

                lastPosition = new Vector3(_camObject.Pos);

            }


        }

        ///////////////////event listeners///////////////////////////////////////


        void OpenGLTess_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (!_enabled) { return; }

            _state = _prevState;
        }

        void OpenGLTess_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!_enabled) return;

            _prevState = _state;

            var state = OpenTK.Input.Keyboard.GetState();

            if (_state != State.NONE)
            {

                return;

            }
            else if (e.Key == Key.A/* event.keyCode == keys[ STATE.ROTATE ]*/ && !_noRotate)
            {

                _state = State.ROTATE;

            }
            else if (e.Key == Key.S /* event.keyCode == keys[ STATE.ZOOM ]*/ && !_noZoom)
            {

                _state = State.ZOOM;

            }
            else if (e.Key == Key.D /* event.keyCode == keys[ STATE.PAN ]*/ && !_noPan)
            {

                _state = State.PAN;

            }

        }

        void OpenGLTess_MouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            if (!_enabled) { return; }

            float delta = 0.0f;

            if (e.Delta != 0)
            { // Firefox

                delta = -(float)e.Delta / 3.0f;

            }

            _zoomStart.Y += delta * 0.05f;
          

        }
        void OpenGLTess_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!_enabled) { return; }

 
            if (_state == State.NONE)
            {
                if (OpenTK.Input.Mouse.GetState()[MouseButton.Right])
                {
                    _state = State.PAN;
                }
                else
                {
                    _state = State.ROTATE;
                }
                //   _state = e.Button;//  event.button;
            }

            if (_state == State.ROTATE && !_noRotate)
            {

                _rotateStart = GetMouseProjectionOnBall(e.X, e.Y);
                _rotateEnd = _rotateStart;

            }
            else if (_state == State.ZOOM && !_noZoom)
            {

                _zoomStart = GetMouseOnScreen(e.X, e.Y);
                _zoomEnd = _zoomStart;


            }
            else if (_state == State.PAN && !_noPan)
            {

                _panStart = GetMouseOnScreen(e.X, e.Y);
                _panEnd = _panStart;



            }

        }

        void OpenGLTess_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (!_enabled) { return; }

        

            _state = State.NONE;
        }

        void OpenGLTess_MouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (!_enabled) { return; }

            if (_state == State.ROTATE && !_noRotate)
            {

                _rotateEnd = GetMouseProjectionOnBall(e.X, e.Y);

            }
            else if (_state == State.ZOOM && !_noZoom)
            {

                _zoomEnd = GetMouseOnScreen(e.X, e.Y);

            }
            else if (_state == State.PAN && !_noPan)
            {

                _panEnd = GetMouseOnScreen(e.X, e.Y);

            }

        }



    }
}
