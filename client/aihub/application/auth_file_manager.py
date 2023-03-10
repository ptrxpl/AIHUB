import json
import os
import click
import sys
from aihub.domain.auth import AuthSettings

class AuthFileManager(object):
    """
    Manages .ai-auth file in the current directory
    """

    AUTH_FILE_PATH = os.path.join(os.getcwd() + "/.ai-auth")

    @classmethod
    def set_auth(self, auth_settings):
        with open(self.AUTH_FILE_PATH, "w") as file:
            file.write(json.dumps(auth_settings.to_dict()))

    @classmethod
    def get_auth(self):
        if not os.path.isfile(self.AUTH_FILE_PATH):
            raise Exception("Missing .ai-auth file")

        with open(self.AUTH_FILE_PATH, "r") as file:
            auth_settings = json.loads(file.read())

        return AuthSettings.from_dict(auth_settings)

    @classmethod
    def exists_auth(self):
        is_init_already = os.path.isfile(self.AUTH_FILE_PATH)
        if is_init_already is False:
            click.echo(click.style('[INFO] Please login first.', fg='yellow'))
            sys.exit()