from marshmallow import Schema, fields, post_load
from aihub.domain.base import BaseModel

class UploadSchema(Schema):
    name = fields.Str()

class UploadConfig(BaseModel):
    schema = UploadSchema()

    def __init__(self, name):
        self.name = name
