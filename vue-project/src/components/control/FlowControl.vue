<script setup>
import { ref, watch } from 'vue'
import { daxHelper } from '../../daxHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    disabled: {
        required: true
    },
    modelValue: {
        required: true
    },
    labelName: {
    },
    attributeName: {
    }
}
)

const selectedName = ref("")
const selectedItem = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => selectedItem.value, (newValue, oldValue) => {
    selectedName.value = newValue?.name;
    emit('update:modelValue', newValue);
})

watch(() => props.modelValue, (newValue, oldValue) => {
    selectedItem.value = newValue
})

function onChange(item) {
    selectedItem.value = null;
}
function handleSelect(item) {
    if (item) {
        selectedItem.value =
        {
            name: item.name,
            uniquename: item.sdkmessagelink_x002e_name,
            id: item.workflowid,
            xaml: item.xaml,
            category: item.category,
            primaryentity: item.primaryentity,
            logicalName: "workflow",
            entitySetName: "workflows",
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
    let attributeXml = `<attribute name='workflowid'/>
                          <attribute name='createdon'/>
                          <attribute name='name'/>
                          <attribute name='uniquename'/>
                          <attribute name='xaml'/>
                          <attribute name='category'/>
                          <attribute name='primaryentity'/>
                          <attribute name='modifiedon'/>`;
    let conditionXml = "";
    if (daxHelper.isGuid(queryString)) {
        conditionXml = `<condition attribute='workflowid' operator='eq' value='{${queryString.replace("{", "").replace("}", "")}}' />`;
    } else if (queryString) {
        conditionXml = `<condition attribute='uniquename' operator='like' value='%${queryString}%' /> <condition attribute='name' operator='like' value='%${queryString}%' />`;
    }
    let fetchXml = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                      <entity name='workflow'>
                           ${attributeXml}
                        <order attribute='modifiedon' descending='true' />
                        <filter type="and">
                           <filter type='or'>
                           ${conditionXml}
                           </filter>
                        <condition attribute="statecode" operator="eq" value="1" />
                        <filter type="or">
                          <condition attribute="category" operator="eq" value="3" />
                          <filter type="and">
                            <condition attribute="category" operator="eq" value="0" />
                            <condition attribute="ondemand" operator="eq" value="1" />
                          </filter>
                        </filter>
                        <condition attribute="type" operator="eq" value="1" />
                      </filter>
                      <link-entity name="sdkmessage" from="sdkmessageid" to="sdkmessageid" link-type="outer" alias="sdkmessagelink">
                      <attribute name="name"/>
                      </link-entity>
                      </entity>
                    </fetch>`;

    return daxHelper.fetch("workflows", fetchXml, true).value;
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-autocomplete :disabled=disabled v-model="selectedName" :fetch-suggestions="querySearch"
            popper-class="my-autocomplete" placeholder="Select a record" @select="handleSelect" @change="onChange">
            <template #suffix>
                <el-icon class="el-input__icon">
                    <Search />
                </el-icon>
            </template>
            <template #default="{ item }">
                <div><span>{{ item.name }} &nbsp;&nbsp;&nbsp;
                        {{ item['category@OData.Community.Display.V1.FormattedValue'] }}</span></div>
                <div><span>{{ item.sdkmessagelink_x002e_name }} &nbsp;&nbsp;&nbsp; {{ item.workflowid }}</span></div>
                <div><span>createdon {{ item['createdon@OData.Community.Display.V1.FormattedValue'] }}
                        &nbsp;&nbsp;&nbsp; modifiedon {{ item['modifiedon@OData.Community.Display.V1.FormattedValue']
                        }}</span></div>
            </template>
        </el-autocomplete>
        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && selectedItem == null" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>