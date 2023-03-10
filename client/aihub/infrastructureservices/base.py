import requests
import client
import sys
from aihub.application.auth_file_manager import AuthFileManager
from log import logger as aihub_logger

class HttpClient(object):
    """
    Base client for all HTTP operations
    """
    def __init__(self):
        self.base_url = client.aihub_host
        #self.base_url = "http://httpbin.org/post" # You can test there responses.

    def request(self,
                method,
                url,
                params=None,
                data=None,
                files=None,
                json=None,
                timeout=5): 
        request_url = self.base_url + url
        #request_url = self.base_url # Uncomment when testing with "http://httpbin.org/post"

        try:
            aihub_logger.debug("Request URL:")
            aihub_logger.debug(request_url)

            access_token_header = ""
            if url != "/users/authenticate":
                auth_settings = AuthFileManager.get_auth()
                access_token = auth_settings['access_token']
                access_token_header = {'Authorization': 'Bearer {}'.format(access_token)}
            else:
                access_token_header = ""

            response = requests.request(method,
                                            request_url,
                                            params=params, # e.g. payload = {'key1': 'value1', 'key2': 'value2'} => r = requests.get('...', params=payload)
                                            data=data, # body - but if you pass json, it will not add Content-Type header!
                                            json=json, # body - json - but don't pass as json.dumps()... - requests will do it for you automatically
                                            files=files,
                                            headers=access_token_header, 
                                            timeout=timeout,
                                            verify='cert.pem',
                                            )

            aihub_logger.debug("Response status code:")
            aihub_logger.debug(response.status_code)

        except requests.exceptions.ConnectionError:
            aihub_logger.info("[ERROR] Cannot connect to the AIHUB server. Check your internet connection.")
            sys.exit()

        except requests.exceptions.Timeout:
            aihub_logger.info("[ERROR] Connection to AIHUB server timed out. Please retry or check your internet connection.")
            sys.exit()

        self.check_response_status(response)
        return response

    def check_response_status(self, response):
        if not (200 <= response.status_code < 300):
            try:
                message = response.json()["result"] # Our response from ASP.NET
            except Exception:
                try:
                    message = response.json()["errors"] # Default response from ASP.NET when [Required] field was empty during request.
                except Exception:
                    message = None                

            aihub_logger.info(f"[ERROR] Received from AIHUB server: status_code: {response.status_code}, message: {message or response.content}")
            sys.exit() #TODO: add Exceptions handling instead of sys.exit()    
        