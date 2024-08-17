<script setup>
import LookUpControl from '../components/control/LookUpControl.vue';
import StringControl from '../components/control/StringControl.vue';
import NumberControl from '../components/control/NumberControl.vue';
import DateControl from '../components/control/DateControl.vue';
import EntityControl from '../components/control/EntityControl.vue';
import PickListControl from '@/components/control/PickListControl.vue';
import { daxHelper } from '../daxHelper.js';
import { ref, watch } from 'vue';
import { ElMessageBox } from 'element-plus'

const selectedEntity = ref(null);
const selectedRecord = ref(null);
const entityDefinition = ref(null);
const attributes = ref([]);
const hiddenAttributes = ['versionnumber', 'utcconversiontimezonecode', 'timezoneruleversionnumber', 'owninguser', 'owningteam', 'owningbusinessunit', 'overriddencreatedon', 'modifiedonbehalfby', 'importsequencenumber', 'createdonbehalfby'];
const disableAttributes = ['statuscode', 'ownerid', 'modifiedby', 'createdon', 'createdby'];
const modelData = ref({});
const modelLookUpData = ref({});
const originalData = ref({});
const saveDialogVisible = ref(false);
const changedData = ref([]);

watch(() => selectedEntity.value, (newValue, oldValue) => {
    selectedRecord.value = null;
    entityDefinition.value = daxHelper.getEntityDefinitionByMetadataId(selectedEntity.value?.MetadataId);
    setAttributes();
})

watch(() => selectedRecord.value, (newValue, oldValue) => {
    lodaSelectedRecordData();
})

function lodaSelectedRecordData() {
    if (!selectedRecord.value) {
        originalData.value = {};
        modelData.value = {};
        modelLookUpData.value = {};
    }
    else {
        originalData.value = daxHelper.retrieve(selectedRecord.value.entitySetName + "(" + selectedRecord.value.id + ")", true);
        modelData.value = daxHelper.createDeepCopy(originalData.value);
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
            modelLookUpData.value[attributeName] = daxHelper.getLookUpModel(originalData.value[attributeName], originalData.value[formattedName], x.Targets);
        }
    })
}
function setFieldChangeData() {
    changedData.value = [];
    attributes.value.forEach(attribute => {
        let displayName = attribute.DisplayName.UserLocalizedLabel?.Label;
        let logicalName = attribute.LogicalName;
        let oldValue = null;
        let newValue = null;
        let inputValue = null;

        if (attribute.AttributeType == "Lookup" || attribute.AttributeType == "Owner") {
            let attributeName = daxHelper.getLookUpWebApiAttributeName(logicalName);
            let formattedName = daxHelper.getFormattedValueString(logicalName);

            if (originalData.value[attributeName] != modelLookUpData.value[attributeName]?.id) {
                if (originalData.value[formattedName]) {
                    oldValue = originalData.value[formattedName] + "(" + originalData.value[attributeName] + ")";
                }
                if (modelLookUpData.value[attributeName] && modelLookUpData.value[attributeName].id) {
                    newValue = modelLookUpData.value[attributeName].name + "(" + modelLookUpData.value[attributeName].id + ")";
                    inputValue = modelLookUpData.value[attributeName].id;
                }
                changedData.value.push({ displayName: displayName, attributeName: logicalName, oldValue: oldValue, newValue: newValue, inputValue: inputValue });
            }

        }
        else if (attribute.AttributeType == "Picklist" || attribute.AttributeType == "Boolean" || attribute.AttributeType == "State" || attribute.AttributeType == "Status" || (attribute.AttributeType == "Virtual" && attribute.AttributeTypeName?.Value == "MultiSelectPicklistType")) {
            if (originalData.value[logicalName] != modelData.value[logicalName]) {
                if (originalData.value[logicalName] != null) {
                    oldValue = daxHelper.getOptionsetLabel(attribute.MetadataId, originalData.value[logicalName]) + "(" + originalData.value[logicalName] + ")";
                }
                if (modelData.value[logicalName] != null) {
                    newValue = daxHelper.getOptionsetLabel(attribute.MetadataId, modelData.value[logicalName]) + "(" + modelData.value[logicalName] + ")";
                    inputValue = modelData.value[logicalName];
                }
                changedData.value.push({ displayName: displayName, attributeName: logicalName, oldValue: oldValue, newValue: newValue, inputValue: inputValue });
            }
        }
        else {
            if (originalData.value[logicalName] != modelData.value[logicalName]) {
                if (originalData.value[logicalName] != null) {
                    oldValue = originalData.value[logicalName];
                }
                if (modelData.value[logicalName] != null) {
                    newValue = modelData.value[logicalName];
                    inputValue = modelData.value[logicalName];
                }
                changedData.value.push({ displayName: displayName, attributeName: logicalName, oldValue: oldValue, newValue: newValue, inputValue: inputValue });
            }
        }
    });
}
function dataChangedByExternal() {
    let versionnumber = daxHelper.retrieve(selectedRecord.value.entitySetName + "(" + selectedRecord.value.id + ")?$select=versionnumber")["versionnumber"];
    return versionnumber != originalData.value["versionnumber"];
}
function dataChangedByExternalConfirm() {
    ElMessageBox.confirm(
        "Do you want to continue saving or reload the data?",
        "The data has been changed externally.",
        {
            distinguishCancelAndClose: true,
            confirmButtonText: "Continue",
            cancelButtonText: "Reload",
            type: "warning",
            center: true,
        }
    )
        .then(() => {
            saveData();
            saveDialogVisible.value = false;
        })
        .catch((action) => {
            if (action == "cancel") {
                lodaSelectedRecordData();
                saveDialogVisible.value = false;
            }
        })
}
function saveBtn() {
    if (selectedRecord.value == null) {
        return;
    }
    setFieldChangeData();
    saveDialogVisible.value = true;
}
function saveConfirmBtn() {
    if (changedData.value.length == 0) {
        lodaSelectedRecordData();
        saveDialogVisible.value = false;
        return;
    }

    if (dataChangedByExternal()) {
        dataChangedByExternalConfirm();
    }
    else {
        saveData();
        saveDialogVisible.value = false;
    }
}
function saveData() {
    lodaSelectedRecordData();
}

</script>

<template>
    <div
        style="display: flex;flex-direction: row;justify-content: center;align-items:center;border-bottom: 1px solid #dddddd;margin-bottom: 20px;">
        <EntityControl labelName="Entity" :required="true" ref="selectEntity" :disabled="false" v-model="selectedEntity">
        </EntityControl>
        <LookUpControl labelName="Record" :logicalName="selectedEntity?.LogicalName" :required="true"
            :disabled="selectedEntity == null" v-model="selectedRecord"></LookUpControl>
        <el-button style="margin-left: 50px;" type="success" @click="saveBtn()">Save</el-button>
    </div>

    <div class="godPage" style="display: flex;flex-wrap: wrap;flex-direction: row;justify-content: center;"
        v-if="selectedRecord != null">
        <div style="width:400px;" v-for="attribute in attributes ">
            <div
                v-if="attribute.AttributeType == 'Picklist' || attribute.AttributeType == 'Boolean' || attribute.AttributeType == 'State' || attribute.AttributeType == 'Status' || (attribute.AttributeType == 'Virtual' && attribute.AttributeTypeName?.Value == 'MultiSelectPicklistType')">
                <PickListControl :labelName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :entityLogicName="selectedEntity.LogicalName" :required="false"
                    :disabled="disableAttributes.includes(attribute.LogicalName)" v-model="modelData[attribute.LogicalName]"
                    :attributeType="attribute.AttributeType" :attributeName="attribute.LogicalName"></PickListControl>
            </div>
            <div
                v-else-if="attribute.AttributeType == 'Double' || attribute.AttributeType == 'Money' || attribute.AttributeType == 'Decimal' || attribute.AttributeType == 'Integer'">
                <NumberControl v-model="modelData[attribute.LogicalName]" :required="false"
                    :labelName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)" :attributeName="attribute.LogicalName">
                </NumberControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'DateTime'">
                <DateControl v-model="modelData[attribute.LogicalName]" :required="false"
                    :labelName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)" :attributeName="attribute.LogicalName">
                </DateControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'String' || attribute.AttributeType == 'Memo'">
                <StringControl v-model="modelData[attribute.LogicalName]" :required="false"
                    :labelName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)" :attributeName="attribute.LogicalName">
                </StringControl>
            </div>
            <div v-else-if="attribute.AttributeType == 'Lookup' || attribute.AttributeType == 'Owner'">
                <LookUpControl :labelName="attribute.DisplayName.UserLocalizedLabel?.Label" :logicalName="attribute.Targets"
                    :required="false" :disabled="disableAttributes.includes(attribute.LogicalName)"
                    v-model="modelLookUpData[daxHelper.getLookUpWebApiAttributeName(attribute.LogicalName)]"
                    :attributeName="attribute.LogicalName">
                </LookUpControl>
            </div>
            <div v-else>
                This Arrtibute Maybe Unsupported:
                <StringControl v-model="modelData[attribute.LogicalName]" :required="false"
                    :labelName="attribute.DisplayName.UserLocalizedLabel?.Label"
                    :disabled="disableAttributes.includes(attribute.LogicalName)" :attributeName="attribute.LogicalName">
                </StringControl>
            </div>
        </div>
    </div>

    <el-dialog v-model="saveDialogVisible" title="Changed Data:">
        <el-table :data="changedData" stripe>
            <el-table-column property="displayName" label="Display Name" width="150" />
            <el-table-column property="attributeName" label="Attribute Name" width="150" />
            <el-table-column property="oldValue" label="Old Value" />
            <el-table-column property="newValue" label="New Value" />
        </el-table>
        <template #footer>
            <span class="dialog-footer">
                <el-button @click="saveDialogVisible = false">Cancel</el-button>
                <el-button type="primary" @click="saveConfirmBtn()">Confirm</el-button>
            </span>
        </template>
    </el-dialog>
</template>
<style scoped>
.godPage:deep(.controlLabel) {
    width: 120px;
    word-break: break-all;
}

.godPage:deep(.controlItem) {
    width: 300px;
}

.godPage:deep(.controlItemTextarea) {
    width: 300px;
}

.godPage:deep(.requiredIcon) {
    visibility: hidden;
}
</style>