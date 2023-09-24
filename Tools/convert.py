import pandas as pd
import io

def isNaN(num):
    return num != num

def process_sheet(sheet):
    print("SIZE:" + str(sheet.shape))
    types = []
    names = []
    data = []

    # extract data from sheet
    for row in sheet.itertuples(index=False):
        values = []
        # extract values in row

        to_ignore = False

        for value in row:
            if str(value)[:2] == "//":
                to_ignore = True
            else:
                if len(types)==0 or len(values) < len(types):
                    values.append(value)

        if len(types)==0:
            types = values
        elif len(names)==0:
            names = values
        else:
            # if the first element is not comment
            if (not isNaN(values[0])) and str(values[0])[:2] != "//" and len(values)==len(names):
                data.append(values)

    return { "types":types, "names":names, "data": data }

def save_to_json(all_sheets):
    #encoding = 'utf-16le'
    encoding = 'utf-8'
    with io.open("definitions.json", 'w', encoding=encoding) as f:
        f.write("{\n")

        for sheet_name, sheet_data in all_sheets.items():
            print("SAVING TO JSON FOR SHEET: " + sheet_name)
            f.write("\t\"" + sheet_name + "\": {\n")

            row_added = False
            types_count = len(sheet_data["types"])

            for row in sheet_data["data"]:
                if row_added:
                    f.write(",\n")
                f.write("\t\t\"" + str(row[0]) + "\": {" "\n")
                
                value_added = False
                for column_no in range(0,types_count):
                    type_name = sheet_data["types"][column_no]
                    name = sheet_data["names"][column_no]
                    value = row[column_no]

                    if value_added:
                        f.write(",\n")
                    f.write("\t\t\t\"" + str(name) + "\" : ")

                    if type_name == "int" or type_name == "float":
                        try:
                            test = float(value)                            
                        except ValueError:
                            raise Exception("Value '" + str(value) + "' in XLSX is not int or float for such type")
                        if isNaN(test):
                            value = "0"
                        f.write(str(value))
                    elif type_name == "HexColor" or type_name == "Color":
                        f.write("\n\t\t\t{\n")
                        r = float(int(value[1:3],16)) / 255.0
                        g = float(int(value[3:5],16)) / 255.0
                        b = float(int(value[5:7],16)) / 255.0
                        if len(value)>7:
                            a = float(int(value[7:9],16)) 
                        else:
                            a = 1.0
                        f.write("\t\t\t\t\"r\": " + str(r) + ",\n")
                        f.write("\t\t\t\t\"g\": " + str(g) + ",\n")
                        f.write("\t\t\t\t\"b\": " + str(b) + ",\n")
                        f.write("\t\t\t\t\"a\": " + str(a) + "\n")
                        f.write("\t\t\t}\n")
                    elif type_name == "bool":                                            
                        if value == False:
                            value = 'false'
                        else:
                            value = 'true'
                        f.write(str(value))
                    elif type_name == "List <string>":             
                        f.write("[")       
                        if isNaN(value):
                            value = ''
                        words = str(value).split(",")  
                        word_added = False
                        for word in words:
                            if word_added:
                                f.write(",")    
                            f.write("\"" + str(word) + "\"")
                            word_added = True
                        f.write("]")       
                    else:
                        # it's like string
                        if str(value) == "nan":
                            value = ""
                        f.write("\"" + str(value) + "\"")
                    value_added = True

                f.write("\n\t\t}")
                row_added = True
   
            f.write("\t}\n")
   
        f.write("}\n")
    pass

def generate_csharp_code(all_sheets):
    #encoding = 'utf-16le'
    encoding = 'utf-8'
    with io.open("Definitions.cs", 'w', encoding=encoding) as f:

        f.write("using System.Collections.Generic;\n\n")
        f.write("using UnityEngine;\n\n")

        for sheet_name, sheet_data in all_sheets.items():
            print("GENERATING CODE FOR SHEET: " + sheet_name)
            f.write("[System.Serializable]\n")
            f.write("public class " + sheet_name + "Definition" + "\n")
            f.write("{\n")

            types_count = len(sheet_data["types"]) 
            for type_no in range(types_count):
                type = sheet_data["types"][type_no]
                if type == "HexColor":
                    type = "Color"
                elif type == "TranslatedString":
                    type = "string"
                f.write("    public " + type + " " + sheet_data["names"][type_no] +";\n")
            f.write("}\n\n")

        # Add Definitions class
        f.write("[System.Serializable]\n")
        f.write("public class Definitions\n")
        f.write("{\n")
        for sheet_name, sheet_data in all_sheets.items():
            f.write("    public Dictionary<string, " + sheet_name + "Definition > " + sheet_name + ";\n")
        f.write("}\n\n")

def main():
    xl = pd.ExcelFile("PlanetsDefinitions.xlsx")
    print(xl.sheet_names)

    all_sheets = {}

    for sheet_name in xl.sheet_names:
        if sheet_name == "END":
            break
            
        print("EXTRACTING SHEET: " + sheet_name)
        sheet = xl.parse(sheet_name)
        sheet_data = process_sheet(sheet)

        #remove spaces
        sheet_name = sheet_name.replace(" ", "")
        all_sheets[sheet_name] = sheet_data

    generate_csharp_code(all_sheets)
    save_to_json(all_sheets)

if __name__ == "__main__":
    # execute only if run as a script
    main()
    