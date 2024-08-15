<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import StringControl from '../components/control/StringControl.vue';
import BoolControl from '../components/control/BoolControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import EntityControl from '../components/control/EntityControl.vue';
import PickListControl from '@/components/control/PickListControl.vue';
import { daxHelper } from '../daxHelper.js';
import { ref, watch } from 'vue';

const selectedEntity = ref(null);
const selectedRecord = ref(null);
const entityDefinition = ref(null);
const attributes = ref([]);
const hiddenAttributes = ['versionnumber', 'utcconversiontimezonecode', 'timezoneruleversionnumber', 'owninguser', 'owningteam', 'owningbusinessunit', 'overriddencreatedon', 'modifiedonbehalfby', 'importsequencenumber', 'createdonbehalfby'];
const disableAttributes = ['statuscode', 'ownerid', 'modifiedby', 'createdon', 'createdby'];
const inputData = ref({});
const inputLookUpData = ref({});
const selectedRecordApiData = ref({});

function handleSelectedRecord() {
    lodaSelectedRecordData();
}

function lodaSelectedRecordData() {
    if (!selectedRecord.value) {
        selectedRecordApiData.value = {};
        inputData.value = {};
        inputLookUpData.value = {};
    }
    else {
        selectedRecordApiData.value = daxHelper.retrieve(selectedRecord.value.entitySetName + '(' + selectedRecord.value.id + ')', true);
        inputData.value = daxHelper.createDeepCopy(selectedRecordApiData.value);
        setInputLookUpData();
    }
}

function setAttributes() {
    attributes.value = [];
    if (entityDefinition.value?.Attributes) {
        //Editable fields are placed at the front.
        attributes.value = entityDefinition.value.Attributes.filter((x) => {
            return x.AttributeOf == null && !x.IsPrimaryId && !hiddenAttributes.includes(x.LogicalName) && !disableAttributes.includes(x.LogicalName);
        })
        //Disable fields are placed at the end.
        attributes.value = attributes.value.concat(entityDefinition.value.Attributes.filter((x) => {
            return x.AttributeOf == null && !x.IsPrimaryId && !hiddenAttributes.includes(x.LogicalName) && disableAttributes.includes(x.LogicalName);
        }))
    }
}

function setInputLookUpData() {
    attributes.value.forEach((x) => {
        if (x.AttributeType == "Lookup" || x.AttributeType == "Owner") {
            let attributeName = daxHelper.getLookUpWebApiAttributeName(x.LogicalName);
            let formattedName = daxHelper.getFormattedValueString(x.LogicalName);
            inputLookUpData.value[attributeName] = daxHelper.getLookUpModel(selectedRecordApiData.value[attributeName], selectedRecordApiData.value[formattedName], x.Targets);
        }
    })
}

watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
    entityDefinition.value = daxHelper.getEntityDefinitionByMetadataId(selectedEntity.value?.MetadataId);
    setAttributes();
})

watch(() => selectedRecord.value, (newValue, oldValue) => {
    handleSelectedRecord();
})

</script>

<template>
    <div
        style="display: flex;flex-direction: row;justify-content: center;align-items:center;border-bottom: 1px solid #dddddd;margin-bottom: 20px;">
        <EntityControl lableName="Entity" :required="true" ref="selectEntity" :disabled="false"
            v-model="selectedEntity">
        </EntityControl>
        <LookUpControl lableName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUpControl>
        <el-button style="margin-left: 50px;" type="success">Save</el-button>
    </div>

    <div class="godPage" style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center;"
        v-if="selectedRecord != null">
        <div style="width:400px;" v-for="attribute in attributes ">
            <div v-if="attribute.AttributeType == 'Boolean'">
                <BoolControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :lableName="attribute.DisplayName.UserLocalizedLabel?.Label" :disabled="disableAttributes.includes(attribute.LogicalName)
                        " :attributeName="attribute.LogicalName"></BoolControl>
            </div>
            <div
                v-else-if="attribute.AttributeType == 'Double' || attribute.AttributeType == 'Money' || attribute.AttributeType == 'Decimal' || attribute.AttributeType == 'Integer'">
                <NumberControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    :attributeName="attribute.LogicalName">
                </NumberControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'DateTime'">
                <DateControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    :attributeName="attribute.LogicalName">
                </DateControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'String' || attribute.AttributeType == 'Memo'">
                <StringControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    :attributeName="attribute.LogicalName">
                </StringControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'Lookup' || attribute.AttributeType == 'Owner'">
                <LookUpControl :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :logicalName="attribute.Targets" :required="false"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    v-model="inputLookUpData[daxHelper.getLookUpWebApiAttributeName(attribute.LogicalName)]"
                    :attributeName="attribute.LogicalName">
                </LookUpControl>
            </div>
            <div
                v-else-if="attribute.AttributeType == 'Picklist' || attribute.AttributeType == 'State' || attribute.AttributeType == 'Status' || (attribute.AttributeType == 'Virtual' && attribute.AttributeTypeName?.Value == 'MultiSelectPicklistType')">
                <PickListControl :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :entityLogicName="selectedEntity.LogicalName" :required="false"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    v-model="inputData[attribute.LogicalName]" :attributeType="attribute.AttributeType"
                    :attributeName="attribute.LogicalName"></PickListControl>
            </div>
            <div v-else>
                This Arrtibute Maybe Unsupported:
                <StringControl v-model="inputData[attribute.LogicalName]" :required="false"
                    :lableName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)"
                    :attributeName="attribute.LogicalName">
                </StringControl>
            </div>
        </div>
    </div>
</template>
<style scoped>
.godPage:deep(.controlLable) {
    width: 120px;
    word-break: break-all;
}

.godPage:deep(.controlItem) {
    width: 300px;
}

.godPage:deep(.controlItemTextarea) {
    width: 300px;
}
</style>