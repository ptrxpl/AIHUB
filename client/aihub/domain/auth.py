from marshmallow import Schema, fields
from aihub.domain.base import BaseModel

class AuthSchema(Schema):
    access_token = fields.Str()

class AuthSettings(BaseModel):
    schema = AuthSchema() # connected with BaseModel

    def __init__(self, access_token):
        self.access_token = access_token