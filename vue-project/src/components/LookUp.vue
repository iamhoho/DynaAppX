<script setup>
import { ref, watch, defineEmits } from 'vue'
import { hocrm } from '../CRMHelper.js'
import { ElMessage } from 'element-plus'

const props = defineProps({
    required: {
        required: true
    },
    disabled: {
        required: true
    },
    logicalName: {
        required: true
    },
    modelValue: {
        required: true
    }
}
)

var selectedName = ref("")
var selectedItem = ref(props.modelValue)
var entityDefinition = ref(null)
watch(() => props.logicalName, (newValue, oldValue) => {
    selectedItem.value = null;
    if (newValue) {
        getEntityDefinition();
    }
})

const emit = defineEmits(['update:modelValue'])

watch(selectedItem, (newValue, oldValue) => {
    selectedName.value = newValue?.name;
    emit('update:modelValue', newValue);
})

function onChange(item) {
    selectedItem.value = null;
}
function handleSelect(item) {
    if (item) {
        selectedItem.value =
        {
            logicalName: entityDefinition.value.LogicalName,
            id: item[entityDefinition.value.PrimaryIdAttribute],
            name: item[entityDefinition.value.PrimaryNameAttribute],
            entitySetName: entityDefinition.value.EntitySetName,
        };
    }
    else {
        selectedItem.value = null
    }
}
function querySearch(queryString, cb) {
    const results = loadData(queryString);
    cb(results);
}
function loadData(queryString) {
    if (!entityDefinition.value) {
        getEntityDefinition();
    }
    if (!entityDefinition.value) {
        ElMessage.error(`"entityDefinition" is missing!`);
        return [];
    }
    return queryEntityDataAuto(queryString);
}
function queryEntityDataAuto(queryString) {
    const queryAttributes = entityDefinition.value.Attributes.filter(getQueryAttributesFilter());
    let attributeXml = `<attribute name='${entityDefinition.value.PrimaryNameAttribute}'/>
                          <attribute name='createdon'/>
                          <attribute name='modifiedon'/>`;
    let conditionXml = "";
    if (hocrm.isGuid(queryString)) {
        conditionXml = `<condition attribute='${entityDefinition.value.PrimaryIdAttribute}' operator='eq' value='{${queryString.replace("{", "").replace("}", "")}}' />`;
    } else if (queryString) {
        queryAttributes.forEach(attribute => {
            conditionXml += `<condition attribute='${attribute.LogicalName}' operator='like' value='%${queryString}%' /> `;
        });
    }
    let fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                      <entity name='${entityDefinition.value.LogicalName}'>
                           ${attributeXml}
                        <order attribute='modifiedon' descending='true' />
                        <filter type='or'>
                           ${conditionXml}
                        </filter>
                      </entity>
                    </fetch>`;

    return hocrm.fetch(entityDefinition.value.EntitySetName, fetchXml, true).value;
}
function getQueryAttributesFilter() {
    return (attribute) => {
        return (
            attribute.IsPrimaryName == true ||
            (attribute.AttributeOf == null && attribute.AttributeType == "String" &&
                (attribute.LogicalName.toLowerCase().indexOf("code") != -1 ||
                    attribute.LogicalName.toLowerCase().indexOf("name") != -1 ||
                    attribute.LogicalName.toLowerCase().indexOf("number") != -1))
        )
    }
}
function getEntityDefinition() {
    let metadataId = hocrm.getEntityDefinitions().filter((item) => {
        return (
            item.LogicalName == props.logicalName
        )
    })[0].MetadataId;
    metadataId = metadataId.replace("{", "").replace("}", "");
    const xhr = new XMLHttpRequest;
    const path = `/api/data/v8.0/EntityDefinitions(${metadataId})?$expand=Attributes`;
    xhr.open("GET", encodeURI(hocrm.getCrmUrl() + path), false);
    xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    xhr.setRequestHeader("OData-MaxVersion", "4.0");
    xhr.setRequestHeader("OData-Version", "4.0");
    xhr.onreadystatechange = (event) => {
        if (event.target.readyState == 4) {
            if (event.target.status == 200) {
                entityDefinition.value = JSON.parse(xhr.responseText);
            } else {
                ElMessage.error(`${xhr.responseURL} ${xhr.status} ${xhr.statusText}`);
            }
        }
    };
    xhr.send();
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <el-autocomplete :disabled=disabled v-model="selectedName" :fetch-suggestions="querySearch"
            popper-class="my-autocomplete" placeholder="Select a record" @select="handleSelect" @change="onChange">
            <template #suffix>
                <el-icon class="el-input__icon">
                    <Search />
                </el-icon>
            </template>
            <template #default="{ item }">
                <div><span>{{ item[entityDefinition.PrimaryNameAttribute] }} &nbsp;&nbsp;&nbsp; {{
                    item[entityDefinition.PrimaryIdAttribute] }}</span></div>
                <div><span>createdon {{ item['createdon@OData.Community.Display.V1.FormattedValue'] }}
                        &nbsp;&nbsp;&nbsp; modifiedon {{ item['modifiedon@OData.Community.Display.V1.FormattedValue']
                        }}</span></div>
            </template>
        </el-autocomplete>
        <el-icon size="2em" :color="required && selectedItem == null ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>