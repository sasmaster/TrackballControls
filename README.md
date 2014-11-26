TrackballControls
=================

C++  and C#(OpenTK) ports of original THREE.js TrackballControls.js by  Eberhard Graether


Reason behind the port:
=================
I worked on a couple of OpenGL C++ and OpenTK (C#) projects where I needed to implement nice trackball mannered 
interactive rotation , zoom and pan.After extensive search of the web I found nothing written in those languages.
Then I found THREE.js TrackballControls utility which was exactly what I needed.

Known bugs,issues:
=================
It is possible that some bugs exists as I still haven't tested the utility extensively.But so far it seems to work ok.
Also,I am sure that firther optimizations on Math instructions can be made,including SIMD integration.


Dependencies:
=================
The C++ version first of all currently depends on GLFW3 (http://www.glfw.org/)
as its window and input managing lib.I haven't had time to add also GLUT,but it should
be really trivial to replace.

GLEW(http://glew.sourceforge.net/) is used for OpenGL context init.

GLM Math(http://glm.g-truc.net/0.9.5/index.html) is used for all math operations.
Important note for those who would like to use it with Direct3D,the lib uses column-major matrix 
order and right-handed coordinate system.So it won't work for Direct3D out of the box.

Usage example:
=================

'''cpp

//Init your GLFW window.

//Init GLEW .
   //init camera object:
    Camera3D tCam(glm::vec3(0.0f,0.0f,100.0f));
	//Init trackball instance :
	TrackballControls* tball = &TrackballControls::GetInstance(&tCam,glm::vec4(0.0f,0.0f,(float)viewportWidth,(float)viewportHeight));
	//Init GLFW callbacks:
	tball->Init(win.GetWindow());
	
	
	//then in a render loop:
	
	while(canRender){
	
	   tball->Update();
	   
	   mat4 myCameraMatrix = tCam.m_viewMatr;
	   
	   //Construct your MVP matrix:
	   	glm::mat4 mvp = perspectiveMatr * tCam.m_viewMatr * modelMatr;
	   
	   //upload mvp to GPU via uniform(don't forget to bind the shader program before):
	   glUniformMatrix4fv(location,1,GL_FALSE,glm::value_ptr(mvp))
	}


'''

