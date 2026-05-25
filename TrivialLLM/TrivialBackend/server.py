from fastapi import FastAPI
from pydantic import BaseModel
import requests
import json

# CARGA DE CONFIGURACION

# Se encarga de leer un archivo de configuracion externo (config.txt)
# Se crea con el objetivo de no escribir directamente en el codigo fuente
# las claves privadas, los endpoints y las URls de las APIs utilizadas
def load_config(path="config.txt"):
    config = {}
    
    # Abrir el archivo en modo lectura
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()

            # Ignorar comentarios, lineas vacias o lineas que comienzan con #
            if not line or line.startswith("#"):
                continue

            # Cada linea valida debe tener el formato CLAVE=VALOR
            # split("=", 1) permite separar solo por el primer "="
            if "=" in line:
                key, value = line.split("=", 1)
                config[key.strip()] = value.strip()

    return config

# Se carga la configuracion desde config.txt
config = load_config("config.txt")

print("CONFIG CARGADO CORRECTAMENTE")

# Lista de las claves obligatorias que deben exisitir en config.txt 
# para que el backend pueda comunicarse con los LLMs
required_keys = ["AZURE_ENDPOINT","AZURE_API_KEY","AZURE_GPT_ENDPOINT", "GEMINI_API_KEY","GEMINI_URL", "CHATGPT_API_KEY","CHATGPT_URL"]

# Comprobar que todas las claves necesarias han sido leidas
for key in required_keys:
    if key in config and config[key]:
        print(f"{key} leída correctamente")
    else:
        print(f"{key} FALTA en config.txt")


# VARIABLES DE CoNFIGURACION DE LOS MODELOS

# Configuracion de Azure OpenAI
AZURE_ENDPOINT = config["AZURE_ENDPOINT"]
AZURE_API_KEY = config["AZURE_API_KEY"]
AZURE_GPT_ENDPOINT = config["AZURE_GPT_ENDPOINT"]

# Configuracion de Gemini
GEMINI_API_KEY = config["GEMINI_API_KEY"]
gemini_url_template = config["GEMINI_URL"]
# Se sustituye {GEMINI_API_KEY} de la URL por la clave real
GEMINI_URL = gemini_url_template.replace("{GEMINI_API_KEY}",config["GEMINI_API_KEY"])

# Configiracion de ChatGPT
CHATGPT_API_KEY = config["CHATGPT_API_KEY"]
CHATGPT_URL = config["CHATGPT_URL"]

# CREACION SERVIDOR

# FastAPI se utiliza para crear un servidor HTTP al que Unity puede enviar
# peticiones. En este proyecto, Unity manda prompts y recibe respuestas 
# generadas por distintos LLMs
app = FastAPI()

# MODELOS DE DATOS RECIBIDOS DESDE UNITY

# Modelo de entrada para el endpoint /trivial
# -prompt: texto de entrada que recibira el modelo
# -model: modelo que se quiere usar: copilot, azure, gemini o chatgpt
# -isAnswering: indica si se esta generando una pregunta o respondiendo una
class PromptRequest(BaseModel):
    prompt: str         
    model: str
    isAnswering: bool

# Modelo de entrada para el endpoint /profile
# Unity envia una descripcion de un rol y el servidor
# genera un perfil estructurado en formato JSON
class RoleRequest(BaseModel):
    role: str

# ENDPOINT PARA GENERAR PERFILES DE JUGADORES

# Recibe un rol en texto y lo transforma en un perfil de jugador de Trivial
# con campos como precision, años de conocimiento, categorias fuertes y debiles, y aleatoridad
@app.post("/profile")
def generate_profile(data: RoleRequest):
    try:
        prompt = f"""
Convierte este rol en JSON válido para un jugador de trivial.

ROL:
{data.role}

INSTRUCCIONES IMPORTANTES:
-Analiza el texto del rol para deducir el 'accuracyBase' (0.0 a 1.0). Si el rol parece torpe o no muy listo, pon un valor bajo. Si parece experto, pon un un valor alto.
-Deduce el 'knowledgeStart' (1940-2025). Año en el que el personaje empezó a aprender conocimientos y hechos del mundo. Si el rol describe a una persona mayor, un anciano o alguien del pasado, asígnale un año de corte antiguo acorde a su época(1940-1960). Si es un adulto, (1960-1980).  Si es un joven o un perfil contemporáneo (1980-2010). Si es un niño pequeño 2020.
-Deduce el 'knowledgeCutoff' (2000-2025). Si el rol describe a una persona mayor, un anciano o alguien del pasado, asígnale un año de corte entre 2000-2020. Si es un adulto o un perfil contemporáneo, asígnale un año cercano a 2025.
-Identifica y genera las listas de temas 'strongCategories' y 'weakCategories'.
-Deduce el 'randomness' (0.0 a 1.0). Si el rol es caótico o loco, asigna un valor alto.
-No uses siempre los mismo valores, adáptalos a la descripción.

Devuelve SOLO JSON válido:
{{
  "accuracyBase": 0.0,
  "knowledgeStart": 2000,
  "knowledgeCutoff": 2025,
  "strongCategories": ["..."],
  "weakCategories": ["..."],
  "randomness": 0.0
}}
"""
        # Cabeceras necesarias para llamar al servicio de Azure
        headers = {
            "Content-Type": "application/json",
            "api-key": AZURE_API_KEY
        }

        # Cuerpo de la peticion enviada al modelo
        # Se usa una temperatura baja para que el resultado sea mas estable
        # y menos aleatorio, ya que se necesita un JSON valido
        body = {
            "messages": [
                {"role": "system", "content": "Eres un generador de perfiles JSON estricto."},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.2
        }

        # Enviar la peticion al endpoint de Azure
        response = requests.post(AZURE_ENDPOINT, headers=headers, json=body)
        result = response.json()

        # Si la respuesta no contiene choices, se devuelve error
        if "choices" not in result:
            return {"error": "No choices in response", "raw": result}

        # Extraer el texto generado por el modelo
        text = result["choices"][0]["message"]["content"]

        # Limpiar posibles ```json para poder parsearlo correctamente
        text = text.replace("```json", "").replace("```", "").strip()

        # Convertir el texto JSON a un objeto Python antes de devolverlo
        return json.loads(text)

    except Exception as e:
        # Cualquier error se devuelve como JSON para que Unity pueda procesarlo
        return {
            "error": str(e)
        }

#  ENDPOINT PRINCIPAL DE TRIVIAL

# Actua como punto de entrada comun para Unity
# Segun el valor de data.model, redirige la peticion al modelo de IA correspondiente
@app.post("/trivial")
def trivial_endpoint(data: PromptRequest):
    if data.model.lower() == "copilot":
        return use_copilot(data.prompt,data.isAnswering)
    elif data.model.lower() == "azure":
        return use_azure(data.prompt,data.isAnswering)
    elif data.model.lower() == "gemini":
        return use_gemini(data.prompt,data.isAnswering)
    elif data.model.lower()=="chatgpt":
        return use_chatgpt(data.prompt,data.isAnswering)
    else:
        return {"error": f"Modelo no soportado: {data.model}"}

# Funcion para usar Azure GPT
# - Genera una pregunta de Trivial en JSON
# - Responde a una pregunta devolviendo solo el indice
def use_azure(prompt: str, isAnswering: bool):
    print("Entrando en use_azure")

    headers = {
        "Content-Type": "application/json",
        "api-key": AZURE_API_KEY
    }

    if not isAnswering:
        # Generacion de preguntas
        # Forzar al modelo a devolver unicamente un JSON valido
        body = {
            "messages": [
                {"role": "system", "content": "Eres un generador de preguntas de trivial. Devuelve ÚNICAMENTE un JSON válido con este formato: {\"pregunta\":\"...\", \"opciones\":[\"...\",\"...\",\"...\",\"...\"], \"respuesta_correcta\":0}. No incluyas markdown ni texto adicional."},
                {"role": "user", "content": prompt}
            ]
            #"temperature": 0.7
        }
    else:
        body = {
            # Respuesta
            # Se pide unicamente el indice de la opcion correcta
            "messages": [
                {"role": "system", "content": "Tienes que contestar a la siguiente pregunta de trivial devolviendo solo el index."},
                {"role": "user", "content": prompt}
            ]
            #"temperature": 0.2
        }
    
    try:
        # Llamada HTTP al endpoint de Azure GPT
        response = requests.post(AZURE_GPT_ENDPOINT, headers=headers, json=body)
        
        # Lanzar excepcion si el servidor devuelve un error HTTP
        response.raise_for_status()

        response_data = response.json()
        respuesta_llm = response_data["choices"][0]["message"]["content"].strip()

        if not isAnswering:
            # Limpiar respuesta
            if respuesta_llm.startswith("```"):
                respuesta_llm = respuesta_llm.replace("```json", "").replace("```", "").strip()
            
            return json.loads(respuesta_llm)
        else:
            # Devolver directamente el indice
            return respuesta_llm

    except Exception as e:
         print(f"Error general en use_azure: {e}")
         return {"error": str(e)}
        
# Funcion para usar Copilot (desde Azure)
# - Genera una pregunta de Trivial en JSON
# - Responde a una pregunta devolviendo solo el indice
def use_copilot(prompt: str, isAnswering: bool):
    print("Entrando en use_copilot")

    headers = {
        "Content-Type": "application/json",
        "api-key": AZURE_API_KEY
    }

    if not isAnswering:
        # Generar pregunta
        body = {
            "messages": [
                {"role": "system", "content": "Eres un generador de preguntas de trivial."},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.7
        }
    else:
        # Responder
        body = {
            "messages": [
                {"role": "system", "content": "Tienes que contestar a la siguiente pregunta de trivial devolviendo solo el index."},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.2
        }

    try:
        # Enviar la peticion al endpoint para Copilot (en Azure)
        response = requests.post(AZURE_ENDPOINT, headers=headers, json=body)
        
        # Lanzar excepcion si hay error HTTP
        response.raise_for_status()

        response_data = response.json()
        respuesta_llm = response_data["choices"][0]["message"]["content"].strip()

        if not isAnswering:
            # Limpiar markdown si es necesario
            if respuesta_llm.startswith("```"):
                respuesta_llm = respuesta_llm.replace("```json", "").replace("```", "").strip()
            
            return json.loads(respuesta_llm)
        else:
            return respuesta_llm

    except Exception as e:
        print(f"Error general en use_copilot: {e}")
        return {"error": str(e)}

# Funcion para usar Gemini
# Utiliza un formato de peticion diferente a Azure.
# En lugar de "messages", se envia un array "contents" con "parts"
def use_gemini(prompt: str, isAnswering: bool):
    print("Entrando en use_gemini")

    headers = {"Content-Type": "application/json"}

    if not isAnswering:
        # Prompt usado para cuando se quiere generar una pregunta
        text = (
            "Genera una pregunta de trivial.\n"
            "Responde SOLO con JSON válido, sin comentarios ni explicaciones.\n"
            '{"pregunta":"", "opciones":["","","",""], "respuesta_correcta":0}\n'
            + prompt
        )
    else:
        # Prompt usado para cuando se quiere responder una pregunta
        text = (
            "Responde a esta pregunta de trivial.\n"
            "Devuelve solo el índice correcto (0-3), sin texto adicional.\n"
            + prompt
        )

    # Cuerpo de la peticion con la configuracion propia de Gemini
    body = {
        "contents": [
            {"parts": [{"text": text}]}
        ],
        "generationConfig": 
        {
            # Mayor temperatura al generar preguntas para mas variedad
            # Menor al responder para aumentar la precision
             "temperature": 0.8 if not isAnswering else 0.2
        }
    }

    try:
        # Llamada a la API de Gemini
        response = requests.post(GEMINI_URL, headers=headers, json=body)
        result_json = response.json()
        print("Respuesta completa Gemini:", result_json)
    except Exception as e:
        print("Error llamando a Gemini:", e)
        return {"error": str(e)}

    # Comprobar que Gemini haya devuelto "candidates"
    # Evita errores KeyError si la API devuelve una respuesta inesperada
    candidates = result_json.get("candidates")
    if not candidates or len(candidates) == 0:
        print("No se recibieron candidates de Gemini")
        return {"error": "No hay candidates", "raw": result_json}

    # Extraer las partes de contenido de la primera respuesta candidata
    content_parts = candidates[0].get("content", {}).get("parts")
    if not content_parts or len(content_parts) == 0:
        print("No hay parts en content de Gemini")
        return {"error": "No hay parts", "raw": candidates[0]}

    texto = content_parts[0].get("text", "")

    if not isAnswering:
        # Limpiar texto
        if texto.startswith("```"):
            texto = texto.replace("```json", "").replace("```", "").strip()

        # Buscar el primer "{" y el ultimo "}" para extraer
        # unicamente el bloque JSON 
        inicio = texto.find("{")
        fin = texto.rfind("}") + 1
        if inicio == -1 or fin == -1:
            print("No se encontró JSON válido en la respuesta")
            return {"error": "JSON no encontrado", "raw": texto}

        json_text = texto[inicio:fin]

        try:
            return json.loads(json_text)
        except json.JSONDecodeError as e:
            print("Error al parsear JSON:", e)
            return {"error": "JSON inválido", "raw": json_text}

    else:
        # Para respuestas, solo devolver indice como string
        # Intentar extraer numero de la respuesta
        texto = texto.strip()
        for token in texto.split():
            if token.isdigit():
                return token

        # Si no hay un numero, devolver texto crudo
        return texto

# Funcion para usar ChatGPT
# Usa la Responses API de OpenAI. 
# Genera y responde preguntas
def use_chatgpt(prompt: str, isAnswering: bool):
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {CHATGPT_API_KEY}"
    }
    if not isAnswering:
        # Generar preguntas
        # Se usa json_schema para obligar al modelo a devolver
        # un JSON con la estructura exacta que espera Unity
        body = {
            "model": "gpt-5-nano",
            "instructions": (
                "Eres un generador de preguntas de trivial. "
                "Devuelve únicamente un JSON válido."
                "Las opciones deben ser texto limpio, sin prefijos como A), B), C), D), ni numeración."
            ),
            "input": prompt,
            "reasoning": {
                "effort" : "minimal"
            },
            "text": {
                "verbosity": "low",
                "format": {
                    "type": "json_schema",
                    "name": "pregunta_trivial",
                    "schema": {
                        "type": "object",
                        "properties": {
                            "pregunta": {"type": "string"},
                            "opciones": {
                                "type": "array",
                                "items": {"type": "string"},
                                "minItems": 4,
                                "maxItems": 4
                            },
                            "respuesta_correcta": {
                                "type": "integer",
                                "minimum": 0,
                                "maximum": 3
                            }
                        },
                        "required": ["pregunta", "opciones", "respuesta_correcta"],
                        "additionalProperties": False
                    },
                    "strict": True
                }
            }
        }
    else:
        # Responder
        # Se pide al modelo que devuelva unicamente el indice de la respuesta correcta
        body = {
            "model": "gpt-5-nano",
            "instructions": (
                "Lee la pregunta y sus opciones. "
                "Devuelve solo un único número: 0, 1, 2 o 3. "
                "No escribas explicación, ni texto adicional, ni signos."
            ),
            "input": prompt,
            "reasoning": {
                "effort": "minimal"
            },
            "text": {
                "verbosity": "low"
            },
            "max_output_tokens": 64
        }
    try:
        # Llamada HTTP a la API de OpenAI
        response = requests.post(CHATGPT_URL, headers=headers, json=body)
        print("STATUS CHATGPT:", response.status_code)
        print("RAW CHATGPT:", response.text)

        # Lanzar excepcion si la respuesta HTTP indica error
        response.raise_for_status()

        response_data = response.json()

        # Si la API indica que la respuesta esta incompleta, se devuelve
        # la informacion detallada para poder depurar desde Unity
        if response_data.get("status") == "incomplete":
            return {
                "error": "Respuesta incompleta de OpenAI",
                "reason": response_data.get("incomplete_details", {}).get("reason"),
                "raw": response_data
            }

        # En la Responses API, el texto final aparece dentro de output
        # Recorrer la estructura hasta encontrar un contenido de tipo output_text
        respuesta_llm = ""

        for item in response_data.get("output", []):
            if item.get("type") == "message":
                for content in item.get("content", []):
                    if content.get("type") == "output_text":
                        respuesta_llm = content.get("text", "").strip()
                        break
            if respuesta_llm:
                break

        # Si no se ha podido extraer texto, se devuelve la respuesta completa
        if not respuesta_llm:
            return {"error": "No se pudo extraer el texto de la respuesta de OpenAI", "raw": response_data}
        
        if not isAnswering:
            # En generacion de preguntas, la respuesta debe ser un JSON valido
            data = json.loads(respuesta_llm)

            # Limpieza adicional para no agregar prefijos a las opciones
            if "opciones" in data and isinstance(data["opciones"], list):
                opciones_limpias = []
                for op in data["opciones"]:
                    if isinstance(op, str):
                        op = op.strip()

                        prefijos = ["A) ", "B) ", "C) ", "D) ",
                                    "A. ", "B. ", "C. ", "D. ",
                                    "A: ", "B: ", "C: ", "D: "]
                        
                        for prefijo in prefijos:
                            if op.startswith(prefijo):
                                op = op[len(prefijo):].strip()
                                break
                    opciones_limpias.append(op)

                data["opciones"] = opciones_limpias
                
            return data
        else:
            # Al responder, se busca el primer caracter que sea un indice valido
            respuesta_llm = respuesta_llm.strip()
            for ch in respuesta_llm:
                if ch in ["0", "1", "2", "3"]:
                    return ch

            # Si no se encuentra ningun indice, se devuelve la respuesta original
            return respuesta_llm

    except Exception as e:
        print(f"Error general en use_chatgpt: {e}")
        return {"error": str(e)}

