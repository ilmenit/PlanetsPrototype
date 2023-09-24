wget -O PlanetsDefinitions.xlsx https://docs.google.com/spreadsheets/d/1vi-HVKCx_78VdbX4K1fld5tR2Z3Tuk_QKbsnl_fPja4/export?format=xlsx
python convert.py
copy definitions.json ..\Assets\StreamingAssets
copy Definitions.cs ..\Assets\Scripts