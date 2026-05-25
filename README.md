# TrivialLLM
Trabajo de Fin de Grado curso 2025-2026. Grado en Desarrollo de Videojuegos, Facultad de Informática UCM.\
Enfréntate a Gemini, ChatGPT y Copilot en una partida de Trivial.

## Manual de usuario
Para probar el proyecto descarga el ejecutable en https://github.com/TFG-2025-26/TrivialLLM/releases/tag/v1.0. Una vez instalado debe
clicarse el archivo _.exe_ para poder ejecutarlo. El proyecto se inicia en el menú principal. Desde esta pantalla, el usuario puede
comenzar una nueva partida seleccionando la opción _Jugar_.

### Configuración de la partida

Al iniciar una nueva partida, se muestra el menú de configuración, donde se
deben definir los siguientes parámetros. Con un mínimo de dos y un máximo de seis
jugadores, se puede combinar:

- Número de jugadores humanos.

- Número de jugadores controlados por inteligencia artificial.

Para los jugadores controlados por humanos es necesario registrar:

- El modelo de lenguaje que se utilizará para generar las preguntas.

Y para los jugadores controlados por IA, es necesario registrar:

- El modelo de lenguaje que se utilizará para generar las preguntas.
- El modelo de lenguaje que se utilizará para responder.
- Opcionalmente, el rol o comportamiento asociado al agente.

Una vez completada la configuración, se inicia la partida.

### Desarrollo de la partida
Durante la partida, los jugadores participan por turnos. En cada turno:
1. Se lanza el dado.
2. El jugador avanza el número de casillas correspondiente.
3. Al llegar a una casilla, se formula una pregunta asociada a su categoría con
una dificultad aleatoria entre fácil, media o difícil.

Las categorías disponibles son:

- Ciencias (verde)
- Arte y literatura (morado)
- Deportes y pasatiempos (naranja)
- Historia (amarillo)
- Geografía (azul)
- Entretenimiento (rosa)

### Condición de victoria
El objetivo del juego es conseguir los seis quesitos, uno por cada categoría. En el juego de mesa original, estos se obtienen al acertar las preguntas en las casillas especiales correspondientes, pero en este proyecto se consigue un quesito en cualquier casilla si se responde correctamente para que las partidas no se alarguen.

Una vez obtenidos todos los quesitos, el jugador debe dirigirse al centro del tablero. Una vez alcanzado el centro, habiendo obtenido el número exacto que necesita al tirar el dado, deberá responder correctamente a una serie de seis preguntas, una por cada categoría y de dificultad aleatoria. Si el jugador acierta al menos cuatro de las seis preguntas, se le considera ganador de la partida. Si falla, debe quedarse en la casilla central e intentarlo en el próximo turno

## Manual de desarrollador
Para abrir el proyecto en Unity, estos son los pasos e instrucciones necesarios. 

### Requisitos técnicos
- Versión 6000.0.58f1 de Unity.
- Versión Python >= 3.8 .
- Cuentas en ChatGPT, Google y Azure Microsoft para conseguir las credenciales privadas de cada API.

### Descargar el proyecto
1. Descarga el proyecto de este repositorio.
2. Extrae el contenido a una carpeta en tu equipo.
3. Abre Unity Hub, selecciona _Add_, después _Add project from disk_ y selecciona
la ruta donde has descargado el proyecto.
4. Cuando aparezca el proyecto en Unity Hub, clicar y esperar a que se abra el
proyecto en Unity.

### Instalación y funcionamiento del backend en local
Las siguientes instrucciones funcionan en Windows. Para abrir una terminal se
deben pulsar las teclas Win + R, después escribir cmd y pulsar la tecla Enter. A
continuación, se deben ejecutar los siguientes comandos:

1. El _backend_ (archivo _server.py_) está hecho en Python, por lo que es necesario disponer de un entorno con Python instalado. Para comprobar si ya está
Python instalado escribir:

        python -–version

        o si eso falla

        python3 -–version

    Si en la consola se ve _Python 3.XX.X_, perfecto. Si no, se debe instalar desde
la web oficial de Python.

2. Localizar la carpeta TrivialBackend siguiendo la ruta  _TrivialLLM\TrivialLLM\TrivialBackend_ 
dentro del repositorio descargado anteriormente. A continuación, en la terminal se debe navegar hasta este directorio utilizando el comando

        cd <ruta_directorio>.
3. Crear y activar un entorno virtual, no es obligatorio pero se recomienda y es
una buena práctica. En la carpeta TrivialBackend, ejecutar

        python -m venv venv

        o

        python3 -m venv venv

    Para activar el entorno escribir:

        venv\Scripts\activate

    Se debería ver (venv) al principio de la línea de la terminal.

4. Este paso puede ser necesario para poder tener permiso para ejecutarlo:

        Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
    
5. Utiliza FastAPI, por lo que se deben instalar las dependencias. Con el entorno
activado, ejecuta:

        pip install fastapi uvicorn requests
6. El archivo _server.py_ debe encontrarse en la carpeta TrivialBackend. Antes
de arrancar el servidor, es necesario configurar las API _keys_ y los _endpoints_
de los modelos que se vayan a utilizar. Para ello, se debe crear el archivo
_config.txt_ en la misma carpeta donde se encuentra _server.py_ y asignar en
él, utilizando los mismos nombres de variables definidos en _server.py_, las
claves correspondientes. Este archivo se ignora en el repositorio, únicamente se encuentra en la máquina local del usuario que realice la ejecución para
almacenar las credenciales propias. El fichero tendría que tener el siguiente
aspecto:

        AZURE_ENDPOINT=
        AZURE_API_KEY=
        AZURE_GPT_ENDPOINT=
        GEMINI_API_KEY=
        GEMINI_URL=
        CHATGPT_API_KEY=
        CHATGPT_URL=

    Se deben incluir todas las variables aunque no se les asigne ningún valor. A
    continuación, se detallan los pasos para obtener y configurar las credenciales:

    #### Configuración de la API de Azure OpenAI:

    - Acceder al Portal de Azure con una cuenta de estudiante, profesional o personal.
    - En la barra de búsqueda buscar Azure OpenAI y crear un recurso,
    ya sea con Foundry o con Azure OpenAI, e implementar el modelo
    deseado.
    - Dentro del recurso, ir a la sección _Keys and Enpoint (Claves y punto
    de conexión)_.
    - En _config.txt_ asignar la API Key a AZURE_API_KEY y el punto
    de conexión/URI de destino específico de los modelos desplegado a
    AZURE_ENDPOINT y a AZURE_GPT_ENDPOINT respectivamente.

    #### Configuración de la API de Gemini:
    - Acceder a Google AI Studio e iniciar sesión con una cuenta de
    Google.
    - Hacer clic en el botón _Crear clave de API_ para generar una clave
    personal.
    - En _config.txt_, asignar la clave generada a GEMINI_API_KEY y la
    URL base utilizada en este proyecto para conectar con el modelo en
    su última versión en GEMINI_URL: https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={GEMINI_API_KEY}.
    - Para poder activar la facturación, en el menú de la izquierda dentro
    del proyecto en la sección _Facturación_ se debe crear o vincular una
    cuenta de facturación añadiendo una tarjeta de crédito o débito. Una
    vez guardada, se asociará esa tarjeta al proyecto. En la sección de
    _Gasto_ se puede establecer un límite de inversión mensual.

    #### Configuración de la API de ChatGPT (OpenAI):
    - Acceder a la plataforma de desarrolladores de OpenAI e iniciar sesión o registrarse.
    - Dirigirse a la sección _API Keys_ y hacer clic en el botón _Create new
    secret key_.
    - Copiar la clave generada (solo visible una vez) y asignarla en config.txt
    a CHATGPT_API_KEY. La URL base de este servicio es https://api.openai.com/v1/responses, la cual también debe asignarse a la
    variable CHATGPT_URL en el archivo config.txt.
    - Activar la facturación en la plataforma de OpenAI, dentro de la organización o proyecto correspondiente en la sección _Home_ y hacer clic
    en el botón _Add credits_. En la sección _Billing_ se activa la facturación
    agregando una tarjeta de crédito. Después, se debe añadir un crédito mínimo inicial para habilitar el uso de la API para poder hacer
    pruebas y enviar peticiones, ya que el plan gratuito no lo permite y
    se producen errores de cuota insuficiente.

7. Una vez configuradas las credenciales propias para los modelos de lenguaje, para ejecutar el servidor, con el entorno virtual activado y dentro de la carpeta,
ejecutar:

        uvicorn server:app --reload --port 8000
8. Si todo ha ido bien el _backend_ debería estar funcionando en : http://localhost:8000/trivial,
y en la consola se debe ver:

        Uvicorn running on http://127.0.0.1:8000
