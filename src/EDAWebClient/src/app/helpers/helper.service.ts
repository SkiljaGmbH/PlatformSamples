import {
  ActivityExecutionTypes,
  ActivityItem,
  DocumentItemType, IndexField,
  MappingItem,
  PropertyItem, ResultObject, ResultStream,
  SignalRLogType
} from '../models/activities.model';

export class HelperService {
  public static transformRequest(data: {}) {
    const requestString = [];
    for (const item in data) {
      if (data.hasOwnProperty(item)) {
        requestString.push(encodeURIComponent(item) + '=' + encodeURIComponent(data[item]));
      }
    }
    return requestString.join('&');
  }

  static getActivityExecutionId(activities: ActivityItem[]): number {
    return activities.find(activity => activity.ExecutionType === ActivityExecutionTypes.INITIALIZER).ActivityInstanceID;
  }

  static getActivityResponderId(activities: ActivityItem[]): number {
    return activities.find(activity => activity.ExecutionType === ActivityExecutionTypes.NOTIFICATION).ActivityInstanceID;
  }

  static buildPropertyList(prefilledCustomValues: string): PropertyItem[] {
    const values = prefilledCustomValues ? prefilledCustomValues.split(', ') : [];
    return values.map(el => {
      return {
        key: el,
        value: null
      };
    });
  }

  static preprocessDocumentTypes(documentTypes: DocumentItemType[]): DocumentItemType[] {
    return documentTypes.map(type => {
      return {
        Name: type.Name,
        ID: type.ID,
        TableDefinitions: type.TableDefinitions,
        ProcessID: type.ProcessID,
        FieldDefinitions: type.FieldDefinitions.map(el => {
          return {
            Name: el.Name,
            Destination: el.Destination || el.Name,
            FieldType: el.FieldType
          };
        })
      };
    });
  }

  static getCustomValues(properties: PropertyItem[]): {Key: string, Value: string}[] {
    return properties.map(el => {
      return {
        Key: el.key,
        Value: el.value
      };
    });
  }

  static postProcessResults(stream: ResultStream, types: DocumentItemType[]): ResultObject {
    const type = types.find(t => t.Name === stream.DocumentType);
    const mappings = type ? type.FieldDefinitions : null;

    return {
      Name: stream.DocumentName,
      Fields: mappings ? mappings.map(mapping => {
        return {
          Name: mapping.Destination || mapping.Name,
          Value: HelperService.getIndexField(stream.IndexFields, mapping) ?
            HelperService.getIndexField(stream.IndexFields, mapping).Value : null
        };
      }) : []
    };
  }

  static getIndexField(indexFields: IndexField[], mapping: MappingItem): IndexField {
    return indexFields.find(field => field.Name === mapping.Name);
  }

  static checkPropertiesNotEmpty(properties: PropertyItem[]): boolean {
    return properties.every(property => !!property.value);
  }

  static checkMappingsNotEmpty(documentTypes: DocumentItemType[]): boolean {
    return documentTypes.every(type => {
      return type.FieldDefinitions.every(mapping => !!mapping.Destination);
    });
  }
}

